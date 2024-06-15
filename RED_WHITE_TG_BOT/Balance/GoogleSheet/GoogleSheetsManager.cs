using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;


namespace Balance.GoogleSheet
{
    internal class GoogleSheetsManager(GoogleCredential credential) : IGoogleSheetsManager
    {
        public Spreadsheet GetSpreadsheet(string googleSpreadsheetIdentifier)
        {
            if(string.IsNullOrEmpty(googleSpreadsheetIdentifier))
                throw new ArgumentNullException(nameof(googleSpreadsheetIdentifier));

            using var sheetsService = new SheetsService(new BaseClientService.Initializer() { HttpClientInitializer = credential });
             return sheetsService.Spreadsheets.Get(googleSpreadsheetIdentifier).Execute();
        }

        public async Task PostSpreadsheetAsync(string googleSpreadsheetIdentifier, ValueRange valueRange, string range)
        {
            if(string.IsNullOrEmpty(googleSpreadsheetIdentifier))
                throw new ArgumentNullException(nameof(googleSpreadsheetIdentifier));

            using var sheetsService = new SheetsService(new BaseClientService.Initializer() { HttpClientInitializer = credential });
            var postRequest = sheetsService.Spreadsheets.Values.Append(valueRange, googleSpreadsheetIdentifier, range);
            postRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            await postRequest.ExecuteAsync();
        }
    }
}
