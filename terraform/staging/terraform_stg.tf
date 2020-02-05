provider "aws" {
  region  = "eu-west-1"
}


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

resource "aws_dynamodb_table" "stg-post-url-table" {
  name           = "stg_PostUrl"
  hash_key       = "url"
  read_capacity  = 5
  write_capacity = 5

  attribute {
    name = "url"
    type = "S"
  }

  tags = {
    Name        = "stg-post-url-table"
    Environment = "staging"
  }
}