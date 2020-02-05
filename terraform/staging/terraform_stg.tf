resource "aws_sqs_queue" "stg-existing-job-post-queue" {
  name                      = "stg_ExistingJobPosts"

  tags = {
    Environment = "staging"
  }
}

resource "aws_sqs_queue" "stg-invalid-job-post-queue" {
  name                      = "stg_InvalidJobPosts"
  
  tags = {
    Environment = "staging"
  }
}

resource "aws_sqs_queue" "stg-job-post-queue" {
  name                      = "stg_JobPosts"
  
  tags = {
    Environment = "staging"
  }
}

resource "aws_sqs_queue" "stg-processed-job-post-queue" {
  name                      = "stg_ProcessedJobPosts"
  
  tags = {
    Environment = "staging"
  }
}


resource "aws_dynamodb_table" "stg-post-body-hashes-table" {
  name           = "stg_PostBodyHashes"
  hash_key       = "sourceHash"
  read_capacity  = 5
  write_capacity = 5

  attribute {
    name = "sourceHash"
    type = "S"
  }

  tags = {
    Name        = "stg-post-body-hashes-table"
    Environment = "staging"
  }
}

resource "aws_dynamodb_table" "stg-post-url-hashes-table" {
  name           = "stg_PostUrlHashes"
  hash_key       = "urlHash"
  read_capacity  = 5
  write_capacity = 5

  attribute {
    name = "urlHash"
    type = "S"
  }

  tags = {
    Name        = "stg-post-url-hashes-table"
    Environment = "staging"
  }
}