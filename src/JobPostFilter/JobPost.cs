using Newtonsoft.Json.Schema;

namespace JobPostFilter
{
    public class JobPost
    {
        public static JSchema GetJsonSchema()
        {
            string jsonString = @"{
                '$id': 'job-post-event.json',
                '$schema': 'http://json-schema.org/draft-07/schema#',
                'description': 'A job post event message',
                'type': 'object',
                'properties': {
                    'sourceId': {
                        'description': 'A unique identifier of the source',
                        'type': 'string'
                    },
                    'sourceType': {
                        'description': 'A scrapper type',
                        'type': 'string'
                    },
                    'scrapperRef': {
                        'description': 'The job post origin',
                        'type': 'string'
                    },
                    'hash': {
                        'description': 'The hash of the job post raw body text',
                        'type': 'string'
                    },
                    's3key': {
                        'description': 'S3 bucket job post object identifier',
                        'type': 'string'
                    },
                    'header': {
                        'description': 'The header of the job post',
                        'type': 'string'
                    },
                    'customer': {
                        'description': 'The company which initiated the job post',
                        'type': 'string'
                    },
                    'keywords': {
                        'description': 'A list of all tech keywords found in the post.',
                        'type': 'array',
                        'minItems': 1,
                        'uniqueItems': true
                    }
                },
                'required': [
                    'sourceId',
                    'sourceType',
                    'hash',
                    's3key',
                    'keywords'
                ],
                'additionalProperties': false
                }";

            return JSchema.Parse(jsonString);
        }
    }
}
