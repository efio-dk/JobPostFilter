using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using ServiceStack.Redis;

namespace JobPostFilter
{
    public class AWSRedis : IDBFacade
    {
        public Task<bool> GetItem(string hash, Table table)
        {
            /*try
            {
                using (var redis = new RedisClient("redis-dss.ia4.0001.use1.cache.amazonaws.com", 6379))
                {
                    redis.Set("foo","br");

                    var allUsers = redis.Get("foo");
                    return true;
                }
            }
            catch (Exception e)
            {
                return "Oops! something went wrong";
            }*/

            throw new System.NotImplementedException();
        }

        public void PutItem(string hash, Table table, string paramName)
        {
            throw new System.NotImplementedException();
        }
    }
}