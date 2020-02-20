using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace JobPostFilter
{
    public class Utility
    {
        public static bool IsSchemaValid(JObject jobPost)
        {
            bool isValid = jobPost.IsValid(JobPost.GetJsonSchema());
            
            return isValid;
        }
    }
}