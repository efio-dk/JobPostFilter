provider "aws" {
  region  = "eu-west-1"
}


resource "aws_sqs_queue" "prod-existing-job-post-queue" {
  name                      = "prod_ExistingJobPosts"

  tags = {
    Environment = "staging"
  }
}

resource "aws_sqs_queue" "prod-invalid-job-post-queue" {
  name                      = "prod_InvalidJobPosts"
  
  tags = {
    Environment = "staging"
  }
}

resource "aws_sqs_queue" "prod-job-post-queue" {
  name                      = "prod_JobPosts"
  
  tags = {
    Environment = "staging"
  }
}

resource "aws_sqs_queue" "prod-processed-job-post-queue" {
  name                      = "prod_ProcessedJobPosts"
  
  tags = {
    Environment = "staging"
  }
}


resource "aws_dynamodb_table" "prod-post-body-hashes-table" {
  name           = "prod_PostBodyHashes"
  hash_key       = "sourceHash"
  read_capacity  = 5
  write_capacity = 5

  attribute {
    name = "sourceHash"
    type = "S"
  }

  tags = {
    Name        = "prod-post-body-hashes-table"
    Environment = "staging"
  }
}

resource "aws_dynamodb_table" "prod-post-url-table" {
  name           = "prod_PostUrl"
  hash_key       = "url"
  read_capacity  = 5
  write_capacity = 5

  attribute {
    name = "url"
    type = "S"
  }

  tags = {
    Name        = "prod-post-url-table"
    Environment = "staging"
  }
}