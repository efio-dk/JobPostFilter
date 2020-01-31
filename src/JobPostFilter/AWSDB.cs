using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;

namespace JobPostFilter
{
    public class AWSDB : IDBFacade
    {
        public async Task<bool> GetItem(string hash, Table table)
        {
            Document result = await table.GetItemAsync(hash);

            return result != null;
        }

        public async void PutItem(string hash, Table table, string paramName)
        {
            Document hashDoc = new Document();
            hashDoc[paramName] = hash;

            await table.PutItemAsync(hashDoc);
        }
    }
}