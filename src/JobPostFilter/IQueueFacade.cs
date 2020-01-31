using System.Threading.Tasks;

namespace JobPostFilter
{
    public interface IQueueFacade
    {
         Task PublishToQueue(string msg, string queueUrl);
    }
}