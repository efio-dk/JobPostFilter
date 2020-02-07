# Region
provider "aws" {
  region = "eu-west-1"
}


# VPC related
resource "aws_vpc" "prod-job-post-vpc" {
  cidr_block = "10.0.0.0/16"

  tags = {
    Name = "prod-job-post-vpc",
    Environment = "production"
  }
}

resource "aws_subnet" "prod-job-post-subnet" {
  vpc_id     = "${aws_vpc.prod-job-post-vpc.id}"
  cidr_block = "10.0.1.0/24"

  tags = {
    Name = "prod-job-post-subnet",
    Environment = "production"
  }
}

resource "aws_internet_gateway" "prod-job-post-internet-gateway" {
  vpc_id = "${aws_vpc.prod-job-post-vpc.id}"

  tags = {
    Name = "prod-job-post-internet-gateway",
    Environment = "production"
  }
}

resource "aws_route_table" "prod-job-post-route-table" {
  vpc_id = "${aws_vpc.prod-job-post-vpc.id}"

  route {
    cidr_block = "10.0.1.0/24"
    gateway_id = "local"
  }

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = "${aws_internet_gateway.prod-job-post-internet-gateway.id}"
  }

  tags = {
    Name = "prod-job-post-route-table",
    Environment = "production"
  }
}

resource "aws_vpc_endpoint" "prod-job-post-sqs-endpoint" {
  vpc_id       = "${aws_vpc.prod-job-post-vpc.id}"
  service_name = "com.amazonaws.eu-west-1.sqs"
  subnet_ids = ["${aws_subnet.prod-job-post-subnet.id}"]

  tags = {
    Environment = "production"
  }
}

resource "aws_vpc_endpoint" "prod-job-post-dynamodb-endpoint" {
  vpc_id       = "${aws_vpc.prod-job-post-vpc.id}"
  service_name = "com.amazonaws.eu-west-1.dynamodb"
  route_table_ids = ["${aws_route_table.prod-job-post-route-table.id}"]

  tags = {
    Environment = "production"
  }
}


# Data resources (SQS, DynamoDB, Redis)
resource "aws_sqs_queue" "prod-existing-job-post-queue" {
  name = "prod_ExistingJobPosts"

  tags = {
    Environment = "production"
  }
}

resource "aws_sqs_queue" "prod-invalid-job-post-queue" {
  name = "prod_InvalidJobPosts"

  tags = {
    Environment = "production"
  }
}

resource "aws_sqs_queue" "prod-job-post-queue" {
  name = "prod_JobPosts"

  tags = {
    Environment = "production"
  }
}

resource "aws_sqs_queue" "prod-processed-job-post-queue" {
  name = "prod_ProcessedJobPosts"

  tags = {
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
}
