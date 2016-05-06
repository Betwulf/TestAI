using System;
using System.Threading.Tasks;
using TestAICore.DataObjects;

namespace TestAICore.Repositories
{
    public interface IDocSource
    {

        Task<bool> DeleteCalcDataWithTrainingCollectionAsync(Action<string> updateMessage);

        Task<bool> DeleteDatabase(Action<string> updateMessage);

        String DatabaseId
        {
            get;
        }



        String CollectionUniverseId
        {
            get;
        }


        IDocCollection<Universe> CollectionUniverse
        {
            get;
        }


        
        String CollectionMarketDataId
        {
            get;
        }

        
        IDocCollection<MarketDataSeries> CollectionMarketData
        {
            get;
        }

        
        String CollectionCalcDataId
        {
            get;
        }



        IDocCollection<CalcDataSeries> CollectionCalcData
        {
            get;
        }

        
        String CollectionCalcDataWithTrainingId
        {
            get;
        }

        
        IDocCollection<CalcDataWithTrainingSeries> CollectionCalcDataWithTraining
        {
            get;
        }

        IDocCollection<AIDataSeries> CollectionAIData
        {
            get;
        }







        String CollectionTradeSimulationId
        {
            get;
        }


        IDocCollection<TradeSimulation> CollectionTradeSimulation
        {
            get;
        }




    }
}
