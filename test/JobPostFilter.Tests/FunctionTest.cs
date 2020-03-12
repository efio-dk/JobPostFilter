using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;
using Moq;
using Amazon.DynamoDBv2.DocumentModel;
using ServiceStack.Redis;
using Amazon.Lambda.TestUtilities;

namespace JobPostFilter.Tests
{
    public class FunctionTest
    {
        // invalid job post json example (missing "sourceType" field which is REQUIRED)
        string invalidJobPostRaw = $@"{{
                    ""sourceId"": ""https://test.321.com"",
                    ""scrapperRef"": ""LinkedIn"",
                    ""hash"": ""9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08"",
                    ""s3key"": ""jobPost1.json"",
                    ""header"": ""DotNet Agency is looking for consultant""
                }}";


        // valid job post json example (contains all REQUIRED fields)
        string validJobPostRaw = $@"{{
                    ""sourceId"": ""https://test.321.com"",
                    ""sourceType"": ""web"",
                    ""scrapperRef"": ""LinkedIn"",
                    ""hash"": ""9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08"",
                    ""s3key"": ""jobPost1.json"",
                    ""header"": ""DotNet Agency is looking for consultant"",
                    ""keywords"": ["".Net"", ""C#""]
                }}";

        static TestLambdaLogger logger = new TestLambdaLogger();
        static TestLambdaContext context = new TestLambdaContext
        {
            Logger = logger
        };

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
        public async Task UnexistentJobPostQueueTest()
        {
            Mock<IDBFacade> db = new Mock<IDBFacade>();
            db.Setup(x => x.ItemExists(It.IsAny<string>(), It.IsAny<Table>())).Returns(Task.FromResult(false));

            var function = new Function();
            JObject jobPostObj = JObject.Parse(validJobPostRaw);

            string queueUri = await function.GetQueueForMessage(jobPostObj, db.Object,context);

            Assert.Equal("https://sqs.eu-west-1.amazonaws.com/833191605868/" + GlobalVars.SUCESS_QUEUE, queueUri);
        }

        [Fact]
        public async Task RepeatedJobPostUrlQueueTest()
        {
            Mock<IDBFacade> db = new Mock<IDBFacade>();
            db.Setup(x => x.ItemExists(It.IsAny<string>(), It.IsAny<Table>())).Returns(Task.FromResult(true));

            var function = new Function();
            JObject jobPostObj = JObject.Parse(validJobPostRaw);

            string queueUri = await function.GetQueueForMessage(jobPostObj, db.Object,context);

            Assert.Equal("https://sqs.eu-west-1.amazonaws.com/833191605868/" + GlobalVars.EXISTING_QUEUE, queueUri);
        }

        [Fact]
        public async Task RepeatedJobPostBodyQueueTest()
        {
            Mock<IDBFacade> db = new Mock<IDBFacade>();
            db.SetupSequence(x => x.ItemExists(It.IsAny<string>(), It.IsAny<Table>())).Returns(Task.FromResult(false)).Returns(Task.FromResult(true));

            var function = new Function();
            JObject jobPostObj = JObject.Parse(validJobPostRaw);

            string queueUri = await function.GetQueueForMessage(jobPostObj, db.Object,context);

            Assert.Equal("https://sqs.eu-west-1.amazonaws.com/833191605868/" + GlobalVars.EXISTING_QUEUE, queueUri);
        }

        [Fact]
        public async Task InvalidJobPostQueueTest()
        {
            Mock<IDBFacade> db = new Mock<IDBFacade>();

            var function = new Function();
            JObject jobPostObj = JObject.Parse(invalidJobPostRaw);

            string queueUri = await function.GetQueueForMessage(jobPostObj, db.Object,context);

            Assert.Equal("https://sqs.eu-west-1.amazonaws.com/833191605868/" + GlobalVars.INVALID_QUEUE, queueUri);
        }
    }
}
