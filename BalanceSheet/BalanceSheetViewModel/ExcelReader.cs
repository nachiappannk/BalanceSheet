using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Nachiappan.BalanceSheetViewModel
{
    public class ExcelReader : IDisposable
    {
        private readonly ILogger _logger;
        private string FileName { get; set; }
        private string SheetName { get; set; }
        private ExcelPackage _package;
        private ExcelWorksheet _sheet;
        private FileStream _stream;

        public ExcelReader(string filename, string sheetName, ILogger logger)
        {
            _logger = logger;
            FileName = filename;
            SheetName = sheetName;
            AssertSheetExists(filename, sheetName);
            _stream = ExcelSheetInfoProvider.GetFileStream(filename);
            _package = new ExcelPackage(_stream);
            _sheet = _package.Workbook.Worksheets[sheetName];
        }

        private static void AssertSheetExists(string excelFileName, string sheetName)
        {
            ExcelSheetInfoProvider sheetInfoProvider = new ExcelSheetInfoProvider(excelFileName);
            if (!sheetInfoProvider.IsSheetPresent(sheetName))
            {
                throw new Exception(sheetName + ": sheet does not exist");
            }
        }


        public T ReadLine<T>(int zeroBasedRowIndex, Func<IRowCellsReader, T> rowToObjectConvertor)
        {

            RowCellsReader rowCellsReader = new RowCellsReader(_sheet, zeroBasedRowIndex, FileName, SheetName, _logger);
            return rowToObjectConvertor.Invoke(rowCellsReader);
        }

        public IList<T> ReadAllLines<T>(int zeroBasedStartRowIndex, Func<IRowCellsReader, T> rowToObjectConvertor)
        {

            var numberOfRows = _sheet.Dimension.Rows;
            List<T> results = new List<T>();
            for (int i = zeroBasedStartRowIndex; zeroBasedStartRowIndex < numberOfRows; zeroBasedStartRowIndex++)
            {
                RowCellsReader rowCellsReader = new RowCellsReader(_sheet, zeroBasedStartRowIndex, FileName, SheetName, _logger);
                results.Add(rowToObjectConvertor.Invoke(rowCellsReader));
            }
            return results;
        }

        public void Dispose()
        {

            _sheet.Dispose();
            _package.Dispose();
            _stream.Dispose();
        }
    }


    public class SheetHeadingVerifier
    {
        public static void VerifyHeadingNames(ILogger logger, ExcelReader reader, List<string> columnNames)
        {
            reader.ReadLine(0,
                r =>
                {
                    string[] ret = new string[columnNames.Count];
                    for (int i = 0; i < columnNames.Count; i++)
                    {
                        var readColumnName = r.ReadString(i);
                        ret[i] = readColumnName;
                        var columnNameOptions = columnNames[i];
                        LogUnmatchedColumnName(logger, r.FileName, r.SheetName, readColumnName, new List<string> { columnNameOptions });
                    }
                    return ret;
                });
        }

        public static void LogUnmatchedColumnName(ILogger logger, string fileName, string sheetName, string columnName, List<string> columnNameOptions)
        {
            var columnNameLower = columnName.ToLower();
            var columnNameOptionsLower = columnNameOptions.Select(x => x.ToLower()).ToList();
            if (!columnNameOptionsLower.Contains(columnNameLower))
            {
                StringBuilder b = new StringBuilder();
                b.AppendLine($"In File {fileName}, ");
                b.AppendLine($"In Sheet {sheetName}, ");
                b.AppendLine($"Expected heading {columnNameOptions.ElementAt(0)} but found {columnName}");
                logger.Log(MessageType.IgnorableError, b.ToString());
            }
        }
    }

    public class ExcelWriter
    {
        private readonly string _fileName;

        public ExcelWriter(string fileName)
        {
            _fileName = fileName;
        }
        public void AddSheet<T>(string sheetName, List<T> rowElements)
        {
            var type = typeof(T);
            var propertiesInfo = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
            propertiesInfo = propertiesInfo.Where(x => x.CanRead).ToList();
            propertiesInfo = propertiesInfo.Where(x => x.GetCustomAttribute<ExcelColumnAttribute>() != null).ToList();
            propertiesInfo = propertiesInfo.OrderBy(x => x.GetCustomAttribute<ExcelColumnAttribute>().Rank).ToList();
            var columnNames = new List<string>();
            var columWidths = new List<double>();
            foreach (var propertyInfo in propertiesInfo)
            {
                var attribute = propertyInfo.GetCustomAttribute<ExcelColumnAttribute>();
                columnNames.Add(attribute.ColumnName);
                columWidths.Add(attribute.ColumnWidth);
            }
            using (ExcelSheetWriter writer = new ExcelSheetWriter(_fileName, sheetName))
            {
                writer.Write(0, columnNames.ToArray());
                writer.SetColumnsWidth(columWidths.ToArray());
                writer.ApplyHeadingFormat(columWidths.Count);
                int rowIndex = 1;
                foreach (var rowElement in rowElements)
                {
                    var objects = new List<object>();
                    foreach (var propertyInfo in propertiesInfo)
                    {
                        objects.Add(propertyInfo.GetValue(rowElement, null));
                    }
                    writer.Write(rowIndex++, objects.ToArray());
                }
            }
        }
    }

    public class ExcelColumnAttribute : Attribute
    {
        public string ColumnName { get; }
        public int ColumnWidth { get; }

        public int Rank { get; }

        public ExcelColumnAttribute(int rank, string columnName, int columnWidth)
        {
            Rank = rank;
            ColumnName = columnName;
            ColumnWidth = columnWidth;
        }
    }

    public class ExcelSheetWriter : IDisposable
    {
        private ExcelPackage _package;
        private ExcelWorksheet _workSheet;
        public ExcelSheetWriter(string fileName, string sheetName)
        {
            FileInfo file = new FileInfo(fileName);
            _package = new ExcelPackage(file);
            _workSheet = _package.Workbook.Worksheets.Add(sheetName);

        }

        public void Write(int zeroBasedRowIndex, object[] values)
        {
            for (int j = 0; j < values.Length; j++)
            {
                int column = j + 1;
                int row = zeroBasedRowIndex + 1;
                _workSheet.Cells[row, column].Value = values[j];
                if (values[j] is double)
                    _workSheet.Cells[row, column].Style.Numberformat.Format = "#,##0.00";
                else if (values[j] is DateTime)
                    _workSheet.Cells[row, column].Style.Numberformat.Format = "dd-mmm-yy";
            }
        }


        public void Write(int zeroBasedRowIndex, string[] values)
        {
            for (int j = 0; j < values.Length; j++)
            {
                int column = j + 1;
                int row = zeroBasedRowIndex + 1;
                _workSheet.Cells[row, column].Value = values[j];
            }
        }

        public void WriteList<T>(int zeroBasedStartingRowIndex, IList<T> items,
            Func<T, int, object[]> itemInterpretor)
        {
            int row = zeroBasedStartingRowIndex;
            foreach (var item in items)
            {
                Write(row++, itemInterpretor.Invoke(item, row));
            }
        }

        public void SetColumnsWidth(params double[] widths)
        {
            for (int i = 1; i <= widths.Length; i++)
            {
                _workSheet.Column(i).Width = widths[i - 1];
            }
        }

        public void ApplyHeadingFormat(int numberOfColumns)
        {
            using (var range = _workSheet.Cells[1, 1, 1, numberOfColumns])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                range.Style.Font.Color.SetColor(Color.White);
                range.AutoFilter = true;
            }
        }

        public void Dispose()
        {
            _package.Save();
            _workSheet.Dispose();
            _package.Dispose();
        }
    }
}
