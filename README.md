# Overview

## Architecture

### Twitter API Consumer

There is a simple console application that connects to and starts streaming tweets from Twitter's sample stream. This console application drops each tweet onto an Azure StorageAccount Queue. That's all the console application does.

### Azure Function

Separately, there is an Azure Functions project. This project has an [Azure queue storage trigger](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger?tabs=in-process%2Cextensionv5&pivots=programming-language-csharp) that picks up the tweets from the queue. This function then stores the tweets ina SQL Server database. The database has an extremely simple structure:

![Database](db.png)

Additionally, the Azure Functions project has an [HTTP trigger](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook-trigger?tabs=in-process%2Cfunctionsv2&pivots=programming-language-csharp) that will return statistics about the tweets that have been.

Make a `GET /stats` request

### Code Organization

The solution has several projects.

- The `Experimental` project was used for my initial efforts to get data from Twitter's API
- The `Models.Twitter` project holds models representing objects supplied from the Twitter API
- The `Setup` project will create database tables and Azure blob storage queues
  - **Note**: this project assumes the database and the Azure blob storage account already exist

# Prerequisites

1. SQL Server (I used SQL Server 2019)
1. .NET Core 3.1
1. Azure Storage Emulator

# Setup

1. Create a new database in your SQL Server instance
1. Supply a connection string to this database in the Setup project's Program.cs file
1. Supply an Azure blob storage account name and key in the Setup project's Program.cs file
