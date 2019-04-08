using System;
using System.Linq;
using Microsoft.Azure.Documents.Client;

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

        public void WriteDocument(DataObjects.Measurement measurement)
        {
            client.CreateDocumentAsync(collectionUri, measurement);
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
            var queryOptions = new FeedOptions {MaxItemCount = 1};

            var sql =
                $"SELECT TOP 1 * FROM TemperatureMeasurements WHERE TemperatureMeasurements.Name = '{name}'  and TemperatureMeasurements.Type = {measurementType} ORDER BY TemperatureMeasurements.UnixTimestamp DESC";
            var measurementsQuery = client.CreateDocumentQuery<DataObjects.TemperatureMeasurement>(
                collectionUri, sql, queryOptions).AsEnumerable().FirstOrDefault();

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