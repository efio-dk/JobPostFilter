using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;

namespace JobPostFilter
{
    public interface IDBFacade
    {
         Task<bool> ItemExists(string hash, Table table);
         void PutItem(string hash, Table table, string paramName);
    }
}