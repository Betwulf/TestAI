using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using TestAICore.DataObjects;
using TestAICore.Repositories;

namespace TestAICore.Managers
{
    public class GetUniverseManager
    {
        private IDocSource mDocDB;
        public GetUniverseManager(IDocSource aDocDB)
        {
            mDocDB = aDocDB;
        }

        public async Task<Universe> GetUniverse(string aUniverseName, Action<string> updateMessage)
        {
            return await Task.Run(() => { var aUniverse = mDocDB.CollectionUniverse.GetById(aUniverseName);
                if (aUniverse == null)
                {
                    updateMessage("Empty Universe, need tickers. Stopping.");
                    return null;
                }
                return aUniverse; });
        }

        public async Task<Universe> CreateUniverse(Action<string> updateMessage)
        {
            // First check if Universe already exists
            Universe u = mDocDB.CollectionUniverse.GetById("S&P500");
            if (u == null)
            {
                Universe temp = JsonConvert.DeserializeObject<Universe>(File.ReadAllText(@"data\snp500.json"));
                temp.Id = temp.UniverseName;
                Universe uni = await mDocDB.CollectionUniverse.CreateAsync(temp);
                updateMessage("Created: " + uni.Id);
                return uni;
            }
            else
            {
                updateMessage("Already there: " + u.Id);
                return u;
            }
        }

    }
}
