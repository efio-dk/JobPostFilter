using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace JobPostFilter
{
    public class Function
    {
        static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
        Table bodyTable = Table.LoadTable(client, "PostBodyHashes");
        Table urlTable = Table.LoadTable(client, "PostUrlHashes");

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {

        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            //JobPost jobPost = JsonConvert.DeserializeObject<JobPost>(message.Body);

            JObject jobPost = JObject.Parse(message.Body);
            bool isValid = jobPost.IsValid(JobPost.GetJsonSchema());

            string jobPostUrl = jobPost.Value<string>("source");
            string jobPostBody = jobPost.Value<string>("rawText");

            string urlHash = ComputeSha256Hash(jobPostUrl);
            bool urlPresent = await GetItem(urlHash, urlTable);

            context.Logger.LogLine(urlHash);
            context.Logger.LogLine(urlPresent.ToString());

            if (urlPresent == false)
            {
                PutItem(urlHash, urlTable, "urlHash");

                string bodyHash = ComputeSha256Hash(jobPostBody);
                bool bodyPresent = await GetItem(bodyHash, bodyTable);

                context.Logger.LogLine(bodyHash);
                context.Logger.LogLine(bodyPresent.ToString());

                if (bodyPresent == false)
                {
                    PutItem(bodyHash, bodyTable, "sourceHash");
                    PublishToQueue(message.Body, "https://sqs.eu-west-1.amazonaws.com/833191605868/ProcessedJobPosts");
                }
                else
                    PublishToQueue(message.Body, "https://sqs.eu-west-1.amazonaws.com/833191605868/ExistingJobPosts");
            }
            else
                PublishToQueue(message.Body, "https://sqs.eu-west-1.amazonaws.com/833191605868/ExistingJobPosts");

            await Task.CompletedTask;
        }

        private string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private async Task<bool> GetItem(string hash, Table table)
        {
            Document result = await table.GetItemAsync(hash);

            return result != null;
        }

        private async void PutItem(string hash, Table table, string paramName)
        {
            Document hashDoc = new Document();
            hashDoc[paramName] = hash;

            await table.PutItemAsync(hashDoc);
        }

        private async void PublishToQueue(string msg, string queueUrl)
        {
            string myQueueURL = queueUrl;
            SendMessageRequest sendMessageRequest = new SendMessageRequest();
            sendMessageRequest.QueueUrl = myQueueURL;
            sendMessageRequest.MessageBody = msg;

            AmazonSQSClient sqsClient = new AmazonSQSClient();

            await sqsClient.SendMessageAsync(sendMessageRequest);
        }
    }
}
