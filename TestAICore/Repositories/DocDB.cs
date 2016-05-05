using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TestAICore.DataObjects;

namespace TestAICore.Repositories
{
    public class DocDB : IDocDB
    {
        private DocumentClient client;
        private DocumentClient Client
        {
            get
            {
                if (client == null)
                {
                    string endpoint = ConfigurationManager.AppSettings["docdb-endpoint"];
                    string authKey = ConfigurationManager.AppSettings["docdb-authKey"];

                    //the UserAgentSuffix on the ConnectionPolicy is being used to enable internal tracking metrics
                    //this is not required when connecting to DocumentDB but could be useful if you, like us, want to run 
                    //some monitoring tools to track usage by application
                    ConnectionPolicy connectionPolicy = new ConnectionPolicy { UserAgentSuffix = Assembly.GetExecutingAssembly().FullName };

                    client = new DocumentClient(new Uri(endpoint), authKey, connectionPolicy);
                }

                return client;
            }
        }

        public async Task<bool> DeleteCalcDataWithTrainingCollectionAsync(Action<string> updateMessage)
        {
            await Client.DeleteDocumentCollectionAsync(CollectionCalcDataWithTraining.SelfLink);
            collectionCalcDataWithTraining = null;
            updateMessage("Deleted CalcDataWithTraining Collection");
            return true;
        }

        public async Task<bool> DeleteDatabase(Action<string> updateMessage)
        {
            await Client.DeleteDatabaseAsync(Database.SelfLink);
            database = null;
            collectionMarketData = null;
            collectionUniverse = null;
            collectionCalcData = null;
            collectionCalcDataWithTraining = null;
            updateMessage("Deleted Database: " + DatabaseId);
            return true;
        }


        private string databaseId;
        public String DatabaseId
        {
            get
            {
                if (string.IsNullOrEmpty(databaseId))
                {

                    databaseId = ConfigurationManager.AppSettings["docdb-database"];
                }

                return databaseId;
            }
        }



        private Database database;
        private Database Database
        {
            get
            {
                if (database == null)
                {
                    database = GetOrCreateDatabase(DatabaseId);
                }
                return database;
            }
        }











        private string collectionUniverseId;
        public String CollectionUniverseId
        {
            get
            {
                if (string.IsNullOrEmpty(collectionUniverseId))
                {

                    collectionUniverseId = ConfigurationManager.AppSettings["docdb-collection-universe"];
                }

                return collectionUniverseId;
            }
        }



        private DocCollection<Universe> collectionUniverse;
        public IDocCollection<Universe> CollectionUniverse
        {
            get
            {
                if (collectionUniverse == null)
                {
                    collectionUniverse = new DocCollection<Universe>(Client, Database.SelfLink, CollectionUniverseId);
                }
                return collectionUniverse;
            }
        }



        private string collectionMarketDataId;
        public String CollectionMarketDataId
        {
            get
            {
                if (string.IsNullOrEmpty(collectionMarketDataId))
                {

                    collectionMarketDataId = ConfigurationManager.AppSettings["docdb-collection-marketdata"];
                }

                return collectionMarketDataId;
            }
        }



        private DocCollection<MarketDataSeries> collectionMarketData;
        public IDocCollection<MarketDataSeries> CollectionMarketData
        {
            get
            {
                if (collectionMarketData == null)
                {
                    collectionMarketData = new DocCollection<MarketDataSeries>(Client, Database.SelfLink, CollectionMarketDataId);
                }
                return collectionMarketData;
            }
        }



        private string collectionCalcDataId;
        public String CollectionCalcDataId
        {
            get
            {
                if (string.IsNullOrEmpty(collectionCalcDataId))
                {

                    collectionCalcDataId = ConfigurationManager.AppSettings["docdb-collection-calcdata"];
                }

                return collectionCalcDataId;
            }
        }



        private DocCollection<CalcDataSeries> collectionCalcData;
        public IDocCollection<CalcDataSeries> CollectionCalcData
        {
            get
            {
                if (collectionCalcData == null)
                {
                    collectionCalcData = new DocCollection<CalcDataSeries>(Client, Database.SelfLink, CollectionCalcDataId);
                }
                return collectionCalcData;
            }
        }


        private string collectionCalcDataWithTrainingId;
        public String CollectionCalcDataWithTrainingId
        {
            get
            {
                if (string.IsNullOrEmpty(collectionCalcDataWithTrainingId))
                {

                    collectionCalcDataWithTrainingId = ConfigurationManager.AppSettings["docdb-collection-calctraindata"];
                }

                return collectionCalcDataWithTrainingId;
            }
        }



        private DocCollection<CalcDataWithTrainingSeries> collectionCalcDataWithTraining;
        public IDocCollection<CalcDataWithTrainingSeries> CollectionCalcDataWithTraining
        {
            get
            {
                if (collectionCalcDataWithTraining == null)
                {
                    collectionCalcDataWithTraining = new DocCollection<CalcDataWithTrainingSeries>(Client, Database.SelfLink, CollectionCalcDataWithTrainingId);
                }
                return collectionCalcDataWithTraining;
            }
        }





        private DocCollection<AIDataSeries> collectionAIData;
        public IDocCollection<AIDataSeries> CollectionAIData
        {
            get
            {
                if (collectionAIData == null)
                {
                    collectionAIData = new DocCollection<AIDataSeries>(Client, Database.SelfLink, CollectionAIDataId);
                }
                return collectionAIData;
            }
        }


        private string collectionAIDataId;
        public String CollectionAIDataId
        {
            get
            {
                if (string.IsNullOrEmpty(collectionAIDataId))
                {

                    collectionAIDataId = ConfigurationManager.AppSettings["docdb-collection-aidata"];
                }

                return collectionAIDataId;
            }
        }





        public string CollectionTradeSimulationId
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IDocCollection<TradeSimulation> CollectionTradeSimulation
        {
            get
            {
                throw new NotImplementedException();
            }
        }







        private Database GetOrCreateDatabase(string databaseId)
        {
            var db = Client.CreateDatabaseQuery()
                            .Where(d => d.Id == databaseId)
                            .AsEnumerable()
                            .FirstOrDefault();

            if (db == null)
            {
                db = client.CreateDatabaseAsync(new Database { Id = databaseId }).Result;
            }

            return db;
        }



    }
}
