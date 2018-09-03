using System;
using System.Linq;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace TempyAPI
{
    public class TempyDB
    {
        private DocumentClient client;
        private readonly Settings settings;
        private Uri collectionUri;

        public TempyDB()
        {
            this.settings = new Settings();
            this.client = new DocumentClient(new Uri(settings.DocumentDBEndpointUri), settings.DocumentDBPrimaryKey);
            collectionUri = UriFactory.CreateDocumentCollectionUri(settings.DocumentDBDatabaseId,settings.DocumentDBCollectionId);

        }

        public void WriteDocument(DataObjects.TemperatureMeasurement tempMeasurement)
        {
            client.CreateDocumentAsync(collectionUri, tempMeasurement);
        }

        
        public IQueryable<DataObjects.TemperatureMeasurement> GetLatestTemperatureMeasurementByName(string name)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            string sql = $"SELECT TOP 1 * FROM TemperatureMeasurements WHERE TemperatureMeasurements.Name = '{name}' ORDER BY TemperatureMeasurements.UnixTimestamp DESC";
            IQueryable<DataObjects.TemperatureMeasurement> measurementsQuery = this.client.CreateDocumentQuery<DataObjects.TemperatureMeasurement>(
                collectionUri,sql,queryOptions);
            

            return measurementsQuery;
        }
        
        public IQueryable<DataObjects.TemperatureMeasurement> GetTemperatureMeasurements()
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            IQueryable<DataObjects.TemperatureMeasurement> measurementsQuery = client.CreateDocumentQuery<DataObjects.TemperatureMeasurement>(collectionUri,queryOptions);
            measurementsQuery = measurementsQuery.OrderByDescending(f => f.UnixTimestamp);

            var count = measurementsQuery.Count();
            return measurementsQuery;
        }
    }
}