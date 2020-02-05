terraform {
 backend "s3" {
 encrypt = true
 bucket = "prod-terraform-job-post-storage-s3"
 region = "eu-west-1"
 key = "terraform.tfstate"
 }
}