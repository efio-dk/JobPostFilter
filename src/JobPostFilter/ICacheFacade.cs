namespace JobPostFilter
{
    public interface ICacheFacade
    {
         bool ItemExists(string hash);
         void PutItem(string hash);
    }
}