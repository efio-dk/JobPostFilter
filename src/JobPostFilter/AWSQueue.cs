using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace JobPostFilter
{
    public class AWSQueue : IQueueFacade
    {
        public async Task PublishToQueue(string msg, string queueUrl)
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