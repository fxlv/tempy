using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Serilog;

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

        public async void WriteDocument(DataObjects.Measurement measurement)
        {
            var s = new Stopwatch();
            s.Start();
            var response = await client.CreateDocumentAsync(collectionUri, measurement);
            s.Stop();
            Log.Debug(
                $"Created document for {measurement.Name}; Time elapsed: {s.Elapsed}; Request charge: {response.RequestCharge}");
        }

        /// <summary>
        ///     Returns the latest measurement with the specified name.
        ///     Measurement type defaults to Temperature (type 0).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="measurementType"></param>
        /// <returns></returns>
        public DataObjects.TemperatureMeasurement GetLatestMeasurementByName(string name,
            int measurementType = 0)
        {
            var s = new Stopwatch();
            s.Start();
            var queryOptions = new FeedOptions {MaxItemCount = 1, PopulateQueryMetrics = true};

            var sql =
                $"SELECT TOP 1 * FROM TemperatureMeasurements WHERE TemperatureMeasurements.Name = '{name}'  and TemperatureMeasurements.Type = {measurementType} ORDER BY TemperatureMeasurements.UnixTimestamp DESC";

            var query = client.CreateDocumentQuery<DataObjects.TemperatureMeasurement>(collectionUri, sql, queryOptions)
                .AsDocumentQuery();

            var result = query.ExecuteNextAsync().GetAwaiter().GetResult();
            DataObjects.TemperatureMeasurement temperatureResult = result.FirstOrDefault();
            var metrics = result.QueryMetrics;
            foreach (var metric in metrics)
            {
                var retryCount = metric.Value.Retries.ToString();
                var totalTime = metric.Value.TotalTime.ToString();
                var retrievedDocumentCount = metric.Value.RetrievedDocumentCount.ToString();
                Log.Debug(
                    $"CosmosDB metrics -> Request charge: {result.RequestCharge}; Retrieved documents: {retrievedDocumentCount}; Retry count: {retryCount}; Total time: {totalTime}");
            }

            s.Stop();
            Log.Debug($"GetLatestMeasurementByName({name},{measurementType}) returned in {s.Elapsed}; ");
            return temperatureResult;
        }

        public List<string> GetNames()
        {
            var s = new Stopwatch();
            s.Start();
            var queryOptions = new FeedOptions {MaxItemCount = -1, PopulateQueryMetrics = true};
            var sql = "SELECT distinct TemperatureMeasurements.Name FROM TemperatureMeasurements";

            var query = client.CreateDocumentQuery<DataObjects.TemperatureMeasurement>(collectionUri, sql, queryOptions)
                .AsDocumentQuery();


            var result = query.ExecuteNextAsync().GetAwaiter().GetResult();

            var names = new List<string>();
            foreach (var v in result) names.Add(v.Name);

            var metrics = result.QueryMetrics;
            foreach (var metric in metrics)
            {
                var retryCount = metric.Value.Retries.ToString();
                var totalTime = metric.Value.TotalTime.ToString();
                var retrievedDocumentCount = metric.Value.RetrievedDocumentCount.ToString();
                Log.Debug(
                    $"CosmosDB metrics -> Request charge: {result.RequestCharge}; Retrieved documents: {retrievedDocumentCount}; Retry count: {retryCount}; Total time: {totalTime}");
            }


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