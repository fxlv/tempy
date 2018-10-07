using System;
using System.Linq;
using Microsoft.Azure.Documents.Client;

namespace TempyAPI
{
    public class TempyDB
    {
        private readonly Settings settings;
        private readonly DocumentClient client;
        private readonly Uri collectionUri;

        public TempyDB()
        {
            settings = new Settings();
            client = new DocumentClient(new Uri(settings.DocumentDBEndpointUri), settings.DocumentDBPrimaryKey);
            collectionUri =
                UriFactory.CreateDocumentCollectionUri(settings.DocumentDBDatabaseId, settings.DocumentDBCollectionId);
        }

        public void WriteDocument(DataObjects.Measurement measurement)
        {
            client.CreateDocumentAsync(collectionUri, measurement);
        }


        public IQueryable<DataObjects.TemperatureMeasurement> GetLatestTemperatureMeasurementByName(string name)
        {
            var queryOptions = new FeedOptions {MaxItemCount = -1};

            var sql =
                $"SELECT TOP 1 * FROM TemperatureMeasurements WHERE TemperatureMeasurements.Name = '{name}' ORDER BY TemperatureMeasurements.UnixTimestamp DESC";
            var measurementsQuery = client.CreateDocumentQuery<DataObjects.TemperatureMeasurement>(
                collectionUri, sql, queryOptions);


            return measurementsQuery;
        }

        public IQueryable<DataObjects.TemperatureMeasurement> GetTemperatureMeasurements()
        {
            var queryOptions = new FeedOptions {MaxItemCount = -1};
            IQueryable<DataObjects.TemperatureMeasurement> measurementsQuery =
                client.CreateDocumentQuery<DataObjects.TemperatureMeasurement>(collectionUri, queryOptions);
            measurementsQuery = measurementsQuery.OrderByDescending(f => f.UnixTimestamp);

            var count = measurementsQuery.Count();
            return measurementsQuery;
        }
    }
}