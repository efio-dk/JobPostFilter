# Region
provider "aws" {
  region = "eu-west-1"
}


# VPC related
resource "aws_vpc" "stg-job-post-vpc" {
  cidr_block = "10.0.0.0/16"

  tags = {
    Name = "stg-job-post-vpc",
    Environment = "staging"
  }
}

resource "aws_subnet" "stg-job-post-subnet" {
  vpc_id     = "${aws_vpc.stg-job-post-vpc.id}"
  cidr_block = "10.0.1.0/24"

  tags = {
    Name = "stg-job-post-subnet",
    Environment = "staging"
  }
}

resource "aws_internet_gateway" "stg-job-post-internet-gateway" {
  vpc_id = "${aws_vpc.stg-job-post-vpc.id}"

  tags = {
    Name = "stg-job-post-internet-gateway",
    Environment = "staging"
  }
}

resource "aws_route_table" "stg-job-post-route-table" {
  vpc_id = "${aws_vpc.stg-job-post-vpc.id}"

  route {
    cidr_block = "10.0.1.0/24"
    gateway_id = "local"
  }

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = "${aws_internet_gateway.stg-job-post-internet-gateway.id}"
  }

  tags = {
    Name = "stg-job-post-route-table",
    Environment = "staging"
  }
}

resource "aws_vpc_endpoint" "stg-job-post-sqs-endpoint" {
  vpc_id       = "${aws_vpc.stg-job-post-vpc.id}"
  service_name = "com.amazonaws.eu-west-1.sqs"
  subnet_ids = ["${aws_subnet.stg-job-post-subnet.id}"]

  tags = {
    Environment = "staging"
  }
}

resource "aws_vpc_endpoint" "stg-job-post-dynamodb-endpoint" {
  vpc_id       = "${aws_vpc.stg-job-post-vpc.id}"
  service_name = "com.amazonaws.eu-west-1.dynamodb"
  route_table_ids = ["${aws_route_table.stg-job-post-route-table.id}"]

  tags = {
    Environment = "staging"
  }
}


# Data resources (SQS, DynamoDB, Redis)
resource "aws_sqs_queue" "stg-existing-job-post-queue" {
  name = "stg_ExistingJobPosts"

  tags = {
    Environment = "staging"
  }
}

resource "aws_sqs_queue" "stg-invalid-job-post-queue" {
  name = "stg_InvalidJobPosts"

  tags = {
    Environment = "staging"
  }
}

resource "aws_sqs_queue" "stg-job-post-queue" {
  name = "stg_JobPosts"

  tags = {
    Environment = "staging"
  }
}

resource "aws_sqs_queue" "stg-processed-job-post-queue" {
  name = "stg_ProcessedJobPosts"

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


resource "aws_elasticache_cluster" "stg-job-post-redis" {
  cluster_id           = "stg-job-post-cluster"
  engine               = "redis"
  node_type            = "cache.t2.small"
  num_cache_nodes      = 1
  parameter_group_name = "default.redis5.0"
  engine_version       = "5.0.6"
  port                 = 6379
}