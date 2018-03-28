using System;
using System.Globalization;
using System.IO;
using System.Text;
using OfficeOpenXml;

namespace Nachiappan.BalanceSheetViewModel.Model.Excel
{
    public class RowCellsReader : IRowCellsReader
    {
        public string FileNameWithPath { get; private set; }
        public string SheetName { get; private set; }
        public string FileName { get; private set; }
        public int LineNumber { get; private set; }

        private readonly ExcelWorksheet _excelWorksheet;
        private readonly int _zeroBasedRowIndex;
        private readonly ILogger _logger;
        private readonly int _rowLimit;
        private readonly int _columnLimit;

        public RowCellsReader(ExcelWorksheet excelWorksheet,
            int zeroBasedRowIndex, string fileName, string sheetName, ILogger logger)
        {
            FileNameWithPath = fileName;
            SheetName = sheetName;
            FileName = Path.GetFileName(FileNameWithPath);
            LineNumber = zeroBasedRowIndex + 1;
            _excelWorksheet = excelWorksheet;
            _zeroBasedRowIndex = zeroBasedRowIndex;
            _logger = logger;

            var excelWorksheetDimension = excelWorksheet.Dimension; 

            //Exception for empty sheet here
            _rowLimit = excelWorksheetDimension.Rows;
            _columnLimit = excelWorksheetDimension.Columns;
        }

        public DateTime ReadDate(int zeroBasedColumnIndex)
        {
            var value = ReadCell(zeroBasedColumnIndex);
            if (value == null)
            {
                var retValue = DateTime.FromOADate(0);
                var errorMessage = $"Expected to find a date but did not, so using {retValue.ToString(CultureInfo.InvariantCulture)}";
                LogError(zeroBasedColumnIndex, errorMessage);
                return retValue;
            }

            if (value is int)
            {
                var retValue = DateTime.FromOADate((int)value);
                var errorMessage = $"Expected to find a date but found a number, so using {retValue.ToString(CultureInfo.InvariantCulture)}";
                LogError(zeroBasedColumnIndex, errorMessage);
                return retValue;
            }

            if (value is double) return DateTime.FromOADate((double)value);

            if (value is string)
            {
                var retValue = Convert.ToDateTime((string)value);
                var errorMessage = $"Expected to find a date but found text, so using {retValue.ToString(CultureInfo.InvariantCulture)}";
                LogError(zeroBasedColumnIndex, errorMessage);
                return retValue;
            }
            if (value is DateTime) return (DateTime)value;

            var returnValue = DateTime.MinValue;
            var errMessage = $"Expected to find a date but did not find any, so using {returnValue.ToString(CultureInfo.InvariantCulture)}";
            LogError(zeroBasedColumnIndex, errMessage);
            return returnValue;
        }

        private void LogError(int zeroBasedColumnIndex, string errorMessage)
        {
            _logger.Log(MessageType.Warning, $"In file {FileName}, ",
                $"In sheet {SheetName}, ",
                $"In cell {GetCellAddress(zeroBasedColumnIndex)}, ",
                errorMessage);
        }

        private object ReadCell(int zeroBasedColumnIndex)
        {
            if (_zeroBasedRowIndex >= _rowLimit) return null;
            if (zeroBasedColumnIndex >= _columnLimit) return null;
            var value = _excelWorksheet.Cells[_zeroBasedRowIndex + 1, zeroBasedColumnIndex + 1].Value;
            return value;
        }

        private string GetCellAddress(int zeroBasedColumnIndex)
        {
            if (_zeroBasedRowIndex >= _rowLimit) return "Un Determined";
            if (zeroBasedColumnIndex >= _columnLimit) return "Un Determined";
            var value = _excelWorksheet.Cells[_zeroBasedRowIndex + 1, zeroBasedColumnIndex + 1].Address;
            return value;
        }

        public int ReadInteger(int zeroBasedColumnIndex)
        {
            var value = ReadCell(zeroBasedColumnIndex);
            if (value == null)
            {
                var retValue = 0;
                var errorMessage = $"Expected to find a number but did not, so using {retValue.ToString(CultureInfo.InvariantCulture)}";
                LogError(zeroBasedColumnIndex, errorMessage);
                return retValue;
            }
            if (value is int) return (int)value;
            if (value is double) return (int)(double)value;
            if (value is string)
            {
                var retValue = Convert.ToInt32((string)value);
                var errorMessage = $"Expected to find a number but found text, so using {retValue.ToString(CultureInfo.InvariantCulture)}";
                LogError(zeroBasedColumnIndex, errorMessage);
                return retValue;
            }
            var returnValue = 0;
            var errMessage = $"Expected to find a number but did not find any, so using {returnValue.ToString(CultureInfo.InvariantCulture)}";
            LogError(zeroBasedColumnIndex, errMessage);
            return returnValue;
        }

        public bool IsValueAvailable(int zeroBasedColumnIndex)
        {
            var value = ReadCell(zeroBasedColumnIndex);
            if (value is string && string.IsNullOrWhiteSpace(value as string)) return false;
            return value != null;
        }

        public double ReadDouble(int zeroBasedColumnIndex)
        {
            var value = ReadCell(zeroBasedColumnIndex);
            if (value == null)
            {
                var retValue = 0;
                var errorMessage = $"Expected to find a number but did not, so using {retValue.ToString(CultureInfo.InvariantCulture)}";
                LogError(zeroBasedColumnIndex, errorMessage);
                return retValue;
            }
            if (value is int) return (int)value;
            if (value is double) return (double)value;
            if (value is string)
            {
                var retValue = Convert.ToDouble((string)value);
                var errorMessage = $"Expected to find a number but found text, so using {retValue.ToString(CultureInfo.InvariantCulture)}";
                LogError(zeroBasedColumnIndex, errorMessage);
                return retValue;
            }

            var returnValue = 0;
            var errMessage = $"Expected to find a number but did not find any, so using {returnValue.ToString(CultureInfo.InvariantCulture)}";
            LogError(zeroBasedColumnIndex, errMessage);
            return returnValue;
        }

        public string ReadString(int zeroBasedColumnIndex)
        {
            var x = ReadCell(zeroBasedColumnIndex);
            return x?.ToString() ?? string.Empty;
        }
    }
}