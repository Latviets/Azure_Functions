# Azure Functions
This project involves working with Azure Functions and Azure Storage (using a local storage emulator).

## Technologies Used
- Azure Services
- Azure Functions
- Azure Blob Storage (Azure.Storage.Blobs)
- Azure Table Storage (Azure.Data.Tables)

## Features
Create a GET API Call: Lists all logs for a specified time period (from/to) using a time trigger.

Fetch Payload from Blob: Retrieves a payload for a specific log entry and stores the success/failure attempt log in the table and the full payload in the blob.

## TimerTrigger - C#
The TimerTrigger makes it incredibly easy to have your functions executed on a schedule. This sample demonstrates a simple use case of calling your function every 5 minutes.

## How It Works
## Azure Function Timer Trigger → GET API Call (List Logs for a Time Period) → Azure Storage Blob (Fetch Payload for Specific Log) → Azure Storage Table (Store Success/Failure Attempt Log)
For a TimerTrigger to work, you provide a schedule in the form of a cron expression. A cron expression is a string with 6 separate fields representing a given schedule via patterns. The pattern used to represent every 5 minutes is 0 */5 * * * *. 

