using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;

namespace JobPostFilter
{
    public class AWSDB : IDBFacade
    {
        ICacheFacade cache;
        public AWSDB()
        {
            cache = new AWSRedis(GlobalVars.REDIS_ENDPOINT, 6379);
        }
        public async Task<bool> ItemExists(string key, Table table)
        {
            bool result = cache.ItemExists(key);

            if (!result)
            {
                Document doc = await table.GetItemAsync(key);
                if(doc != null)
                    result = true;
            }

            return result;
        }

        public async void PutItem(string key, Table table, string paramName)
        {
            cache.PutItem(key);
            
            Document doc = new Document();
            doc[paramName] = key;

            await table.PutItemAsync(doc);
        }
    }
}