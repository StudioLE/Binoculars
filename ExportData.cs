﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Dynamo.Graph.Workspaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Binoculars
{
    public static class ExportData
    {
        internal static string user;
        internal static string computerName;
        internal static string dynamoVersion;
        internal static string revitVersion;
        internal static string ip;
        internal static string latlng;
        internal static string city;
        internal static string country;
        internal static string filename;
        private static string date;

        internal static IList<IList<object>> Export()
        {
            IList<IList<object>> export = new List<IList<object>>();

            // Set the date
            // @todo "hh" is incorrectly returning values in the afternoon. It should be "14" not "2" etc for any hour after midday
            date = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

            // Create and return a list of all the ExportData strings
            export.Add(new List<object> { user, computerName, ip, latlng, city, country, dynamoVersion, revitVersion, filename, date } );
            return export;
        }
    }

    public static class ExportSheets
    {        
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Google Sheets API .NET Quickstart";

        public static void Execute(IList<IList<object>> list)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "users",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define request parameters.
            // @todo Fetch these from a config.json file or environment variables?
            String spreadsheetId = "1-NNRsKhonKzNmTrl2H3IfwIZvGTy0HMs6AvXhsw-nUc";
            String spreadsheetTab = "Test Data";

            // Define the sheet range
            var rng = string.Format("{0}!A1:A{1}", spreadsheetTab, list.Count);
            var vRange = new ValueRange
            {
                Range = rng,
                Values = list,
                MajorDimension = "ROWS"
            };

            // Send the request to the Google Sheets API
            var rqst = service.Spreadsheets.Values.Append(vRange, spreadsheetId, rng);
            rqst.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            rqst.Execute();
            // @todo We need to check the request was actually sent successfully and gracefully deal with cases where the API is unavailable
        }
    }
}
