using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace Nachiappan.BalanceSheetViewModel.Model.Excel
{
    public class ExcelSheetInfoProvider
    {
        private readonly string _excelFileName;

        public ExcelSheetInfoProvider(string excelFileName)
        {
            _excelFileName = excelFileName;
        }

        public IList<string> GetSheetNames()
        {
            AssertFileExists(_excelFileName);
            AssertFileExtentionIsXlsx(_excelFileName);
            using (Stream stream = GetFileStream(_excelFileName))
            {
                using (var package = new ExcelPackage(stream))
                {
                    return package.Workbook.Worksheets.Select(s => s.Name).ToList();
                }
            }
        }

        private static void AssertFileExtentionIsXlsx(string excelFileName)
        {
            if (Path.GetExtension(excelFileName) != ".xlsx")
            {
                throw new Exception("The extention is not xlsx");
            }
        }

        private static void AssertFileExists(string excelFileName)
        {
            if (!File.Exists(excelFileName))
            {
                throw new Exception("File Does Not Exist");
            }
        }

        public static FileStream GetFileStream(string excelFileName)
        {
            FileStream stream = File.Open(excelFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return stream;
        }
    }

    public static class ExcelSheetInfoProviderExt
    {
        public static bool IsSheetPresent(this ExcelSheetInfoProvider infoProvider, string sheetName)
        {
            return infoProvider.GetSheetNames().Select(x => x.ToLower()).Contains(sheetName.ToLower());
        }
    }
}