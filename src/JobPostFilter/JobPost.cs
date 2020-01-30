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
                    'source': {
                    'description': 'The name of source',
                    'type': 'string'
                    },
                    'sourceId': {
                    'description': 'A unique identifier of the source',
                    'type': 'string'
                    },
                    'timestamp': {
                    'description': 'A timestamp identifying when the job post was scrapped',
                    'type':'string',
                    'format' : 'date-time'
                    },
                    'headline': {
                    'description': 'The job post headline',
                    'type': 'string'
                    },
                    'startTime': {
                    'description': 'First day of work',
                    'type':'string',
                    'format' : 'date-time'
                    },
                    'endTime': {
                    'description': 'Expected last day of work',
                    'type':'string',
                    'format' : 'date-time'
                    },
                    'location': {
                    'description':'Where in the world is this job taking place.',
                    'type':'string'
                    },
                    'type': {
                    'description': 'The type of job.',
                    'type': 'array',
                    'items':{
                        'type': 'string',
                        'enum': [
                        'Consultant',
                        'Permanent',
                        'Full-time',
                        'Part-time',
                        'On-site',
                        'Remote',
                        'Fixed price'
                        ]
                    }
                    },
                    'keywords': {
                    'description': 'A list of all tech keywords found in the post.',
                    'type': 'array',
                    'minItems': '1',
                    'uniqueItems': true
                    },
                    'customer': {
                    'description': 'Name of the End Customer.',
                    'type': 'string'
                    },
                    'agency': {
                    'description': 'Name of the agency that is man-in-the-middle.',
                    'type': 'string'
                    },
                    'rawText': {
                    'description': 'The raw job post',
                    'type': 'string'
                    }
                },
                'required': [ 'source', 'sourceId', 'timestamp', 'headline', 'rawText' ],
                'additionalProperties': false
                }";

            return JSchema.Parse(jsonString);
        }
    }
}