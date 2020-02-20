# Region
provider "aws" {
  region = "eu-west-1"
}


# VPC related
resource "aws_vpc" "prod-job-post-vpc" {
  cidr_block           = "10.0.0.0/16"
  enable_dns_hostnames = true

  tags = {
    Name        = "prod-job-post-vpc",
    Environment = "production"
  }
}

resource "aws_subnet" "prod-job-post-subnet" {
  vpc_id     = aws_vpc.prod-job-post-vpc.id
  cidr_block = "10.0.1.0/24"

  tags = {
    Name        = "prod-job-post-subnet",
    Environment = "production"
  }
}

resource "aws_internet_gateway" "prod-job-post-internet-gateway" {
  vpc_id = aws_vpc.prod-job-post-vpc.id

  tags = {
    Name        = "prod-job-post-internet-gateway",
    Environment = "production"
  }
}

resource "aws_route_table" "prod-job-post-route-table" {
  vpc_id = aws_vpc.prod-job-post-vpc.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.prod-job-post-internet-gateway.id
  }

  tags = {
    Name        = "prod-job-post-route-table",
    Environment = "production"
  }
}

resource "aws_vpc_endpoint" "prod-job-post-sqs-endpoint" {
  vpc_id              = aws_vpc.prod-job-post-vpc.id
  service_name        = "com.amazonaws.eu-west-1.sqs"
  subnet_ids          = ["${aws_subnet.prod-job-post-subnet.id}"]
  security_group_ids  = ["${aws_security_group.prod-job-post-security-group.id}"]
  vpc_endpoint_type   = "Interface"
  private_dns_enabled = true

  tags = {
    Name        = "prod-job-post-sqs-endpoint",
    Environment = "production"
  }
}

resource "aws_vpc_endpoint" "prod-job-post-dynamodb-endpoint" {
  vpc_id            = aws_vpc.prod-job-post-vpc.id
  service_name      = "com.amazonaws.eu-west-1.dynamodb"
  vpc_endpoint_type = "Gateway"
  route_table_ids   = ["${aws_route_table.prod-job-post-route-table.id}"]

  tags = {
    Name        = "prod-job-post-dynamodb-endpoint",
    Environment = "production"
  }
}

resource "aws_elasticache_subnet_group" "prod-job-post-subnet-group" {
  name       = "prod-job-post-subnet-group"
  subnet_ids = ["${aws_subnet.prod-job-post-subnet.id}"]
}

resource "aws_security_group" "prod-job-post-security-group" {
  name        = "prod-job-post-security-group"
  description = "Secure all job post related traffic"
  vpc_id      = aws_vpc.prod-job-post-vpc.id

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
resource "aws_sqs_queue" "prod-existing-job-post-queue" {
  name = "prod_ExistingJobPosts"

  tags = {
    Name        = "prod-existing-job-post-queue",
    Environment = "production"
  }
}

resource "aws_sqs_queue" "prod-invalid-job-post-queue" {
  name = "prod_InvalidJobPosts"

  tags = {
    Name        = "prod-invalid-job-post-queue",
    Environment = "production"
  }
}

resource "aws_sqs_queue" "prod-processed-job-post-queue" {
  name = "prod_ProcessedJobPosts"

  tags = {
    Name        = "prod-processed-job-post-queue",
    Environment = "production"
  }
}

resource "aws_sqs_queue" "prod-incoming-job-post-queue" {
  name = "prod_JobPosts"

  tags = {
    Name        = "prod-incoming-job-post-queue",
    Environment = "production"
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
    Environment = "production"
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
    Environment = "production"
  }
}


resource "aws_elasticache_cluster" "prod-job-post-redis" {
  cluster_id           = "prod-job-post-cluster"
  engine               = "redis"
  node_type            = "cache.t2.small"
  num_cache_nodes      = 1
  parameter_group_name = "default.redis5.0"
  engine_version       = "5.0.6"
  port                 = 6379
  subnet_group_name    = "prod-job-post-subnet-group"
  security_group_ids   = ["${aws_security_group.prod-job-post-security-group.id}"]

  tags = {
    Name        = "prod-job-post-redis"
    Environment = "production"
  }
}

# Lambda
resource "aws_lambda_function" "prod-JobPostFilter-lambda" {
  function_name    = "prod-JobPostFilter"
  handler          = "JobPostFilter::JobPostFilter.Function::FunctionHandler"
  runtime          = "dotnetcore2.1"
  role             = "arn:aws:iam::833191605868:role/JobPostFilterRole"
  filename         = "../../src/JobPostFilter/bin/Release/netcoreapp2.1/JobPostFilter.zip"
  source_code_hash = filebase64sha256("../../src/JobPostFilter/bin/Release/netcoreapp2.1/JobPostFilter.zip")
  timeout          = 10

  vpc_config {
    subnet_ids         = ["${aws_subnet.prod-job-post-subnet.id}"]
    security_group_ids = ["${aws_security_group.prod-job-post-security-group.id}"]
  }

  tags = {
    Name        = "prod-JobPostFilter"
    Environment = "production"
  }
}

resource "aws_lambda_event_source_mapping" "prod-incoming-sqs" {
  event_source_arn = aws_sqs_queue.prod-incoming-job-post-queue.arn
  function_name    = aws_lambda_function.prod-JobPostFilter-lambda.arn
}