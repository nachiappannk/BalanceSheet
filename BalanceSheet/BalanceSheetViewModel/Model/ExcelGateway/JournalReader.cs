using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Nachiappan.BalanceSheetViewModel.Model.Statements;

namespace Nachiappan.BalanceSheetViewModel.Model.ExcelGateway
{
    public class JournalReader
    {
        public static List<JournalStatement> ReadJournalStatements(string fileName, string sheetName, ILogger logger)
        {
            
            JournalGateway gateway = new JournalGateway(fileName);
            var journalStatements = gateway.GetJournalStatements(logger, sheetName);
            AdjustDate(journalStatements);
            AdjustAccount(journalStatements);
            AdjustDescription(journalStatements);
            AdjustTag(journalStatements);
            AdjustAccountNesting(journalStatements);
            return journalStatements;
        }

        private static void AdjustDate(List<JournalStatement> statements)
        {
            statements.ForEach(x => x.Date = x.Date.Date);
        }

        private static void AdjustAccount(JournalStatement statement)
        {
            if(string.IsNullOrWhiteSpace(statement.Account)) statement.Account = String.Empty;
        }

        private static void AdjustDescription(JournalStatement statement)
        {
            if (string.IsNullOrWhiteSpace(statement.Description)) statement.Description = String.Empty;
        }

        private static void AdjustAccount(List<JournalStatement> journalStatements)
        {
            journalStatements.ForEach(AdjustAccount);
            journalStatements.ForEach(x => x.Account = x.Account.Trim());
            journalStatements.ForEach(x => x.Account = x.Account.ToLower());
            journalStatements.ForEach(x => x.Account = Regex.Replace(x.Account, @"\s+", " "));
        }

        private static void AdjustDescription(List<JournalStatement> journalStatements)
        {
            journalStatements.ForEach(AdjustDescription);
            journalStatements.ForEach(x => x.Description = x.Description.Trim());
        }

        private static void AdjustTag(List<JournalStatement> journalStatements)
        {
            journalStatements.ForEach(AdjustTag);
            journalStatements.ForEach(x => x.Tag = x.Tag.Trim());
        }

        private static void AdjustTag(JournalStatement x)
        {
            if (string.IsNullOrWhiteSpace(x.Tag)) x.Tag = string.Empty;
        }

        private static void AdjustAccountNesting(List<JournalStatement> journalStatements)
        {
            journalStatements.ForEach(x => x.Account = x.Account.Replace(@"\", "/"));
        }
    }
}