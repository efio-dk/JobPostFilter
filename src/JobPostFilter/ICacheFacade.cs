namespace JobPostFilter
{
    public interface ICacheFacade
    {
         bool GetItem(string hash);
         void PutItem(string hash);
    }
}