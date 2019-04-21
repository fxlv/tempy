using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json.Serialization;
using RestSharp.Extensions;
using Serilog;
using Document = System.Reflection.Metadata.Document;

namespace TempyAPI
{
    public class TempyDbAuthCredentials
    {
        public string DocumentDBEndpointUri { get; set; }
        public string DocumentDBPrimaryKey { get; set; }
        public string DocumentDBDatabaseId { get; set; }
        public string DocumentDBCollectionId { get; set; }
    }

    public class TempyDB
    {
        private readonly DocumentClient client;
        private readonly Uri collectionUri;

        public TempyDB(TempyDbAuthCredentials authCredentials)
        {
            client = new DocumentClient(new Uri(authCredentials.DocumentDBEndpointUri),
                authCredentials.DocumentDBPrimaryKey);
            collectionUri =
                UriFactory.CreateDocumentCollectionUri(authCredentials.DocumentDBDatabaseId,
                    authCredentials.DocumentDBCollectionId);
        }

        public  async void WriteDocument(DataObjects.Measurement measurement)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            var response = await client.CreateDocumentAsync(collectionUri, measurement);
            s.Stop();
            Log.Debug($"Created document for {measurement.Name}; Time elapsed: {s.Elapsed}; Request charge: {response.RequestCharge}");
        }

        /// <summary>
        /// Returns the latest measurement with the specified name.
        /// Measurement type defaults to Temperature (type 0).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="measurementType"></param>
        /// <returns></returns>
        public DataObjects.TemperatureMeasurement GetLatestMeasurementByName(string name,
            int measurementType = 0)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            var queryOptions = new FeedOptions {MaxItemCount = 1};

            var sql =
                $"SELECT TOP 1 * FROM TemperatureMeasurements WHERE TemperatureMeasurements.Name = '{name}'  and TemperatureMeasurements.Type = {measurementType} ORDER BY TemperatureMeasurements.UnixTimestamp DESC";
            var measurementsQuery = client.CreateDocumentQuery<DataObjects.TemperatureMeasurement>(
                collectionUri, sql, queryOptions).AsEnumerable().FirstOrDefault();
            s.Stop();
            Log.Debug($"GetLatestMeasurementByName({name},{measurementType}) returned in {s.Elapsed}");
            return measurementsQuery;
        }

        public List<string> GetNames()
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            var queryOptions = new FeedOptions {MaxItemCount = -1};
            var sql = "SELECT distinct TemperatureMeasurements.Name FROM TemperatureMeasurements";
            IQueryable<dynamic> measurementsQuery =
                client.CreateDocumentQuery<DataObjects.TemperatureMeasurement>(collectionUri, sql, queryOptions);
            List<string> names = new List<string>();
            foreach (var v in measurementsQuery)
            {
                names.Add(v.Name);
            }

            //var offer = client.ReadOfferAsync().Result;
           s.Stop();
            Log.Debug($"Time elapsed in GetNames() is {s.Elapsed}");
            return names;
        }

        public IQueryable<DataObjects.TemperatureMeasurement> GetTemperatureMeasurements()
        {
            var queryOptions = new FeedOptions {MaxItemCount = -1};
            IQueryable<DataObjects.TemperatureMeasurement> measurementsQuery =
                client.CreateDocumentQuery<DataObjects.TemperatureMeasurement>(collectionUri, queryOptions);
            measurementsQuery = measurementsQuery.OrderByDescending(f => f.UnixTimestamp);

            return measurementsQuery;
        }
    }
}