namespace JobPostFilter
{
    public static class GlobalVars
    {
        #if DEBUG
            // debug stuff goes here
            public static readonly string EXISTING_QUEUE = "stg_ExistingJobPosts";
            public static readonly string SUCESS_QUEUE = "stg_ProcessedJobPosts";
            public static readonly string INVALID_QUEUE = "stg_InvalidJobPosts";
            public static readonly string BODY_TABLE = "stg_PostBodyHashes";
            public static readonly string URL_TABLE = "stg_PostUrl";
        #else
        // release stuff goes here
            public static readonly string EXISTING_QUEUE = "prod_ExistingJobPosts";
            public static readonly string SUCESS_QUEUE = "prod_ProcessedJobPosts";
            public static readonly string INVALID_QUEUE = "prod_InvalidJobPosts";
            public static readonly string BODY_TABLE = "prod_PostBodyHashes";
            public static readonly string URL_TABLE = "prod_PostUrl";
        #endif
    }
}