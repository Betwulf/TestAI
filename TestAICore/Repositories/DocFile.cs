using System;
using System.Configuration;
using System.Threading.Tasks;
using TestAICore.DataObjects;

namespace TestAICore.Repositories
{
    public class DocFile : IDocSource
    {
        private string mRootDir;

        public DocFile()
        {
            mRootDir = ConfigurationManager.AppSettings["docdb-rootdir"];
        }




        private DocCollectionFile<AIDataSeries> collectionAIData;
        public IDocCollection<AIDataSeries> CollectionAIData
        {
            get
            {
                if (collectionAIData == null)
                {
                    collectionAIData = new DocCollectionFile<AIDataSeries>(mRootDir);
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




        private DocCollectionFile<CalcDataSeries> collectionCalcData;
        public IDocCollection<CalcDataSeries> CollectionCalcData
        {
            get
            {
                if (collectionCalcData == null)
                {
                    collectionCalcData = new DocCollectionFile<CalcDataSeries>(mRootDir);
                }
                return collectionCalcData;
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



        private DocCollectionFile<CalcDataWithTrainingSeries> collectionCalcDataWithTraining;
        public IDocCollection<CalcDataWithTrainingSeries> CollectionCalcDataWithTraining
        {
            get
            {
                if (collectionCalcDataWithTraining == null)
                {
                    collectionCalcDataWithTraining = new DocCollectionFile<CalcDataWithTrainingSeries>(mRootDir);
                }
                return collectionCalcDataWithTraining;
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

        private DocCollectionFile<MarketDataSeries> collectionMarketData;
        public IDocCollection<MarketDataSeries> CollectionMarketData
        {
            get
            {
                if (collectionMarketData == null)
                {
                    collectionMarketData = new DocCollectionFile<MarketDataSeries>(mRootDir);
                }
                return collectionMarketData;
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

        private DocCollectionFile<Universe> collectionUniverse;
        public IDocCollection<Universe> CollectionUniverse
        {
            get
            {
                if (collectionUniverse == null)
                {
                    collectionUniverse = new DocCollectionFile<Universe>(mRootDir);
                }
                return collectionUniverse;
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



        private string collectionTradeSimulationId;

        public string CollectionTradeSimulationId
        {
            get
            {
                if (string.IsNullOrEmpty(collectionTradeSimulationId))
                {

                    collectionTradeSimulationId = ConfigurationManager.AppSettings["docdb-collection-tradesim"];
                }

                return collectionTradeSimulationId;
            }
        }

        private DocCollectionFile<TradeSimulation> collectionTradeSimulation;
        public IDocCollection<TradeSimulation> CollectionTradeSimulation
        {
            get
            {
                if (collectionTradeSimulation == null)
                {
                    collectionTradeSimulation = new DocCollectionFile<TradeSimulation>(mRootDir);
                }
                return collectionTradeSimulation;
            }
        }




        public async Task<bool> DeleteCalcDataWithTrainingCollectionAsync(Action<string> updateMessage)
        {
            return await Task.Run(() =>
            {
                var ct = CollectionCalcDataWithTraining.CollectionId;
                collectionCalcDataWithTraining = null;

                var cc = CollectionCalcData.CollectionId;
                collectionCalcData.DeleteDatabase();
                collectionCalcData = null;
                updateMessage("Deleted CalcDataWithTraining Collection");
                return true;
            });
        }

        public async Task<bool> DeleteDatabase(Action<string> updateMessage)
        {
            return await Task.Run(() =>
           {
               collectionCalcDataWithTraining.DeleteDatabase();
               collectionCalcDataWithTraining = null;

               collectionCalcData.DeleteDatabase();
               collectionCalcData = null;

               collectionMarketData.DeleteDatabase();
               collectionMarketData = null;

               collectionUniverse.DeleteDatabase();
               collectionUniverse = null;

               collectionAIData.DeleteDatabase();
               collectionAIData = null;


               updateMessage("Deleted Database: " + DatabaseId);
               return true;
           });
        }
    }
}
