using System;
using System.Collections.Generic;
using TestAICore.DataObjects;


namespace TestAICore.DataSources
{
    public interface IDataCaller
    {
        List<MarketData> GetHistoricalData(string aTicker, DateTime startdate, DateTime enddate, Action<string> updateMessage);
    }
}
