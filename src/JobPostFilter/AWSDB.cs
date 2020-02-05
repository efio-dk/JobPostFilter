using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;

namespace JobPostFilter
{
    public class AWSDB : IDBFacade
    {
        public async Task<bool> GetItem(string key, Table table)
        {
            Document result = await table.GetItemAsync(key);

            return result != null;
        }

        public async void PutItem(string key, Table table, string paramName)
        {
            Document doc = new Document();
            doc[paramName] = key;

            await table.PutItemAsync(doc);
        }
    }
}