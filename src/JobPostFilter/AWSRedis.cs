using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using ServiceStack.Redis;

namespace JobPostFilter
{
    public class AWSRedis : ICacheFacade
    {
        private string _redisURL;
        private int _redisPort;

        public AWSRedis(string redisURL, int port)
        {
            _redisURL = redisURL;
            _redisPort = port;
        }

        public bool ItemExists(string hash)
        {
            bool exists = false;
            using (var client = new RedisClient(_redisURL, _redisPort))
            {
                exists = client.ContainsKey(hash);
            }
            return exists;
        }

        public void PutItem(string hash)
        {
            using (var client = new RedisClient(_redisURL, _redisPort))
            {
                try
                {
                    client.SetValue(hash, "");
                }
                catch (Exception e)
                { }
            }
        }
    }
}