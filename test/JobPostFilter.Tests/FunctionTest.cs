using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.SQSEvents;

using JobPostFilter;
using Newtonsoft.Json.Linq;
using Xunit;
using Moq;
using Amazon.DynamoDBv2.DocumentModel;

namespace JobPostFilter.Tests
{
    public class FunctionTest
    {
        // invalid job post json example (missing "source" field which is REQUIRED)
        string invalidJobPostRaw = $@"{{
                    ""sourceId"": ""https://test.321.com"",
                    ""timestamp"": ""2020-01-13T20:20:39+00:00"",
                    ""headline"": ""The most successful job"",
                    ""startTime"": ""2020-02-01T08:30:00+00:00"",
                    ""endTime"": ""2021-02-01T16:30:00+00:00"",
                    ""location"": ""Denmark"",
                    ""type"": [""Consultant"", ""Full-time"", ""On-site"", ""Fixed price""],
                    ""keywords"": [""c#"", ""dotnet"", ""full stack""],
                    ""customer"": ""TDC"",
                    ""agency"": ""DotNet Agency"",
                    ""rawText"": ""This is a job body post. You are welcome to send us CVs.Super different""
                }}";


        // valid job post json example (contains all REQUIRED fields)
            string validJobPostRaw = $@"{{
                    ""source"": ""1"",
                    ""sourceId"": ""https://test.321.com"",
                    ""timestamp"": ""2020-01-13T20:20:39+00:00"",
                    ""headline"": ""The most successful job"",
                    ""startTime"": ""2020-02-01T08:30:00+00:00"",
                    ""endTime"": ""2021-02-01T16:30:00+00:00"",
                    ""location"": ""Denmark"",
                    ""type"": [""Consultant"", ""Full-time"", ""On-site"", ""Fixed price""],
                    ""keywords"": [""c#"", ""dotnet"", ""full stack""],
                    ""customer"": ""TDC"",
                    ""agency"": ""DotNet Agency"",
                    ""rawText"": ""This is a job body post. You are welcome to send us CVs.Super different""
                }}";

        /*[Fact]
        public async Task TestSQSEventLambdaFunction()
        {
            /*var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = "foobar"
                    }
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var function = new Function();
            await function.FunctionHandler(sqsEvent, context);

            Assert.Contains("Processed message foobar", logger.Buffer.ToString());
        }*/

        [Fact]
        public void InvalidJobPostTest()
        {
            JObject jobPostObj = JObject.Parse(invalidJobPostRaw);
            bool isValid = Utility.IsSchemaValid(jobPostObj);

            Assert.False(isValid);
        }

        [Fact]
        public void ValidJobPostTest()
        {
            JObject jobPostObj = JObject.Parse(validJobPostRaw);
            bool isValid = Utility.IsSchemaValid(jobPostObj);

            Assert.True(isValid);
        }

        [Fact]
        public void CreatesValidShaTest()
        {
            string textToSha = "testsha";
            string expectedSha = "caa312fbbcf8ff3213b917d4232bca39aae7740338791114072f07ff3692ca72";

            string computedSha = Utility.ComputeSha256Hash(textToSha);

            Assert.Equal(expectedSha, computedSha);
        }

        [Fact]
        public async Task UnexistentJobPostQueueTest()
        {
            Mock<IDBFacade> db = new Mock<IDBFacade>();
            db.Setup(x => x.GetItem(It.IsAny<string>(), It.IsAny<Table>())).Returns(Task.FromResult(false));

            var function = new Function();
            JObject jobPostObj = JObject.Parse(validJobPostRaw);

            string queueUri = await function.GetQueueForMessage(jobPostObj, db.Object);

            Assert.Equal("https://sqs.eu-west-1.amazonaws.com/833191605868/ProcessedJobPosts", queueUri);
        }

        [Fact]
        public async Task RepeatedJobPostUrlQueueTest()
        {
            Mock<IDBFacade> db = new Mock<IDBFacade>();
            db.Setup(x => x.GetItem(It.IsAny<string>(), It.IsAny<Table>())).Returns(Task.FromResult(true));

            var function = new Function();
            JObject jobPostObj = JObject.Parse(validJobPostRaw);

            string queueUri = await function.GetQueueForMessage(jobPostObj, db.Object);

            Assert.Equal("https://sqs.eu-west-1.amazonaws.com/833191605868/ExistingJobPosts", queueUri);
        }

        [Fact]
        public async Task RepeatedJobPostBodyQueueTest()
        {
            Mock<IDBFacade> db = new Mock<IDBFacade>();
            db.SetupSequence(x => x.GetItem(It.IsAny<string>(), It.IsAny<Table>())).Returns(Task.FromResult(false)).Returns(Task.FromResult(true));

            var function = new Function();
            JObject jobPostObj = JObject.Parse(validJobPostRaw);

            string queueUri = await function.GetQueueForMessage(jobPostObj, db.Object);

            Assert.Equal("https://sqs.eu-west-1.amazonaws.com/833191605868/ExistingJobPosts", queueUri);
        }

        [Fact]
        public async Task InvalidJobPostQueueTest()
        {
            Mock<IDBFacade> db = new Mock<IDBFacade>();

            var function = new Function();
            JObject jobPostObj = JObject.Parse(invalidJobPostRaw);

            string queueUri = await function.GetQueueForMessage(jobPostObj, db.Object);

            Assert.Equal("https://sqs.eu-west-1.amazonaws.com/833191605868/InvalidJobPosts", queueUri);
        }
    }
}
