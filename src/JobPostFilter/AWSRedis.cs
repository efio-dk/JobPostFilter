using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using ServiceStack.Redis;

namespace JobPostFilter
{
    public class AWSRedis : ICacheFacade
    {
        public bool GetItem(string hash)
        {
            bool exists = false;
            var manager = new RedisManagerPool("jobpostfilter-redis.ovby8n.ng.0001.euw1.cache.amazonaws.com:6379");
            using ( var client = manager.GetClient())
            {
                try {
                    client.GetValue(hash);
                    exists = true;
                }
                catch(Exception e)
                {
                    exists = false;
                }
            }
            return exists;
        }

        public void PutItem(string hash)
        {
            var manager = new RedisManagerPool("jobpostfilter-redis.ovby8n.ng.0001.euw1.cache.amazonaws.com:6379");
            using ( var client = manager.GetClient())
            {
                try {
                    client.SetValue(hash,"");
                }
                catch(Exception e)
                {}
            }
        }
    }
}