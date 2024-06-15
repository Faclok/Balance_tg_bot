
using Google.Apis.Sheets.v4.Data;

namespace Balance.GoogleSheet
{
    public interface IGoogleSheetsManager
    {
        public Spreadsheet GetSpreadsheet(string document);

        public Task PostSpreadsheetAsync(string googleSpreadsheetIdentifier, ValueRange valueRange, string range);
    }
}
