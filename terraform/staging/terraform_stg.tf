# Region
provider "aws" {
  region = "eu-west-1"
}


# VPC related
resource "aws_vpc" "stg-job-post-vpc" {
  cidr_block           = "10.0.0.0/16"
  enable_dns_hostnames = true

  tags = {
    Name        = "stg-job-post-vpc",
    Environment = "staging"
  }
}

resource "aws_subnet" "stg-job-post-subnet" {
  vpc_id     = aws_vpc.stg-job-post-vpc.id
  cidr_block = "10.0.1.0/24"

  tags = {
    Name        = "stg-job-post-subnet",
    Environment = "staging"
  }
}

resource "aws_internet_gateway" "stg-job-post-internet-gateway" {
  vpc_id = aws_vpc.stg-job-post-vpc.id

  tags = {
    Name        = "stg-job-post-internet-gateway",
    Environment = "staging"
  }
}

resource "aws_route_table" "stg-job-post-route-table" {
  vpc_id = aws_vpc.stg-job-post-vpc.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.stg-job-post-internet-gateway.id
  }

  tags = {
    Name        = "stg-job-post-route-table",
    Environment = "staging"
  }
}

resource "aws_vpc_endpoint" "stg-job-post-sqs-endpoint" {
  vpc_id              = aws_vpc.stg-job-post-vpc.id
  service_name        = "com.amazonaws.eu-west-1.sqs"
  subnet_ids          = ["${aws_subnet.stg-job-post-subnet.id}"]
  security_group_ids  = ["${aws_security_group.stg-job-post-security-group.id}"]
  vpc_endpoint_type   = "Interface"
  private_dns_enabled = true

  tags = {
    Name        = "stg-job-post-sqs-endpoint",
    Environment = "staging"
  }
}

resource "aws_vpc_endpoint" "stg-job-post-dynamodb-endpoint" {
  vpc_id            = aws_vpc.stg-job-post-vpc.id
  service_name      = "com.amazonaws.eu-west-1.dynamodb"
  vpc_endpoint_type = "Gateway"
  route_table_ids   = ["${aws_route_table.stg-job-post-route-table.id}"]

  tags = {
    Name        = "stg-job-post-dynamodb-endpoint",
    Environment = "staging"
  }
}

resource "aws_elasticache_subnet_group" "stg-job-post-subnet-group" {
  name       = "stg-job-post-subnet-group"
  subnet_ids = ["${aws_subnet.stg-job-post-subnet.id}"]
}

resource "aws_security_group" "stg-job-post-security-group" {
  name        = "stg-job-post-security-group"
  description = "Secure all job post related traffic"
  vpc_id      = aws_vpc.stg-job-post-vpc.id

  ingress {
    from_port = 0
    to_port   = 0
    protocol  = "-1"
    self      = true
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}


# Data resources (SQS, DynamoDB, Redis)
resource "aws_sqs_queue" "stg-existing-job-post-queue" {
  name = "stg_ExistingJobPosts"

  tags = {
    Name        = "stg-existing-job-post-queue",
    Environment = "staging"
  }
}

resource "aws_sqs_queue" "stg-invalid-job-post-queue" {
  name = "stg_InvalidJobPosts"

  tags = {
    Name        = "stg-invalid-job-post-queue",
    Environment = "staging"
  }
}

resource "aws_sqs_queue" "stg-processed-job-post-queue" {
  name = "stg_ProcessedJobPosts"

  tags = {
    Name        = "stg-processed-job-post-queue",
    Environment = "staging"
  }
}

resource "aws_sqs_queue" "stg-incoming-job-post-queue" {
  name = "stg_JobPosts"

  tags = {
    Name        = "stg-incoming-job-post-queue",
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


resource "aws_elasticache_cluster" "stg-job-post-redis" {
  cluster_id           = "stg-job-post-cluster"
  engine               = "redis"
  node_type            = "cache.t2.small"
  num_cache_nodes      = 1
  parameter_group_name = "default.redis5.0"
  engine_version       = "5.0.6"
  port                 = 6379
  subnet_group_name    = "stg-job-post-subnet-group"
  security_group_ids   = ["${aws_security_group.stg-job-post-security-group.id}"]

  tags = {
    Name        = "stg-job-post-redis"
    Environment = "staging"
  }
}

# Lambda
resource "aws_lambda_function" "stg-JobPostFilter-lambda" {
  function_name    = "stg-JobPostFilter"
  handler          = "JobPostFilter::JobPostFilter.Function::FunctionHandler"
  runtime          = "dotnetcore2.1"
  role             = "arn:aws:iam::833191605868:role/JobPostFilterRole"
  filename         = "../../src/JobPostFilter/bin/Release/netcoreapp2.1/JobPostFilter.zip"
  source_code_hash = filebase64sha256("../../src/JobPostFilter/bin/Release/netcoreapp2.1/JobPostFilter.zip")
  timeout          = 10

  vpc_config {
    subnet_ids         = ["${aws_subnet.stg-job-post-subnet.id}"]
    security_group_ids = ["${aws_security_group.stg-job-post-security-group.id}"]
  }

  tags = {
    Name        = "stg-JobPostFilter"
    Environment = "staging"
  }
}

resource "aws_lambda_event_source_mapping" "stg-incoming-sqs" {
  event_source_arn = aws_sqs_queue.stg-incoming-job-post-queue.arn
  function_name    = aws_lambda_function.stg-JobPostFilter-lambda.arn
}