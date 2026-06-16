# Invoice Generator API

A learning project demonstrating async/await patterns and background job processing using .NET 10. The API generates PDF invoices asynchronously, stores them in Azure Blob Storage with tags, and provides endpoints to check job status and download completed invoices.

## Features

- **Async Job Processing**: Non-blocking invoice generation workflow
- **In-Memory Queue**: Uses `System.Threading.Channels` for efficient job queueing
- **Background Service**: Processes jobs asynchronously without blocking API requests
- **Azure Blob Storage**: Persists PDF invoices with tag-based retrieval
- **Bogus Data Generation**: Creates realistic fake invoices for testing
- **PDF Generation**: Converts invoice data to PDF using templates
- **Global Exception Handling**: Centralized error handling middleware
- **OpenAPI/Swagger**: Interactive API documentation

## API Endpoints

### 1. Generate Invoice
Creates a new invoice generation job and queues it for processing.

```
POST /invoices/generate
```

**Response** (202 Accepted):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Queued",
  "link": "https://api.example.com/invoices/550e8400-e29b-41d4-a716-446655440000/status"
}
```

### 2. Check Job Status
Retrieves the current status of an invoice generation job.

```
GET /invoices/{id}/status
```

**Response** (200 OK):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Processing"
}
```

When completed, includes a download link:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Completed",
  "link": "https://api.example.com/invoices/550e8400-e29b-41d4-a716-446655440000"
}
```

### 3. Download Invoice
Downloads the generated invoice PDF once processing is complete.

```
GET /invoices/{id}
```

**Response** (200 OK): PDF file

**Status Codes**:
- `404 Not Found`: Invoice ID not found
- `400 Bad Request`: Invoice generation not yet completed

## Architecture

```
┌─────────────────────────────────────────────────────┐
│ InvoicesController                                  │
│ - POST /generate → Enqueue job (202 Accepted)       │
│ - GET /{id}/status → Check status (200 OK)          │
│ - GET /{id} → Download invoice (200 OK)             │
└─────────────────────────────────────────────────────┘
		 ↓
┌─────────────────────────────────────────────────────┐
│ Channel<InvoiceGenerationJob>                       │
│ - Bounded channel (capacity: 50)                     │
│ - Prevents memory overflow                          │
└─────────────────────────────────────────────────────┘
		 ↓
┌─────────────────────────────────────────────────────┐
│ InvoiceGenerationService (BackgroundService)        │
│ - Dequeues jobs asynchronously                       │
│ - Orchestrates processing pipeline                  │
│ - Updates job status dictionary                     │
└─────────────────────────────────────────────────────┘
		 ↓
┌─────────────────────────────────────────────────────┐
│ InvoiceFactory + PdfGenerator                       │
│ - Generates fake invoice data (Bogus)               │
│ - Renders PDF from template                         │
└─────────────────────────────────────────────────────┘
		 ↓
┌─────────────────────────────────────────────────────┐
│ AzureBlobStorage                                    │
│ - Uploads PDF with tags                             │
│ - Retrieves by tag-based query                      │
└─────────────────────────────────────────────────────┘
```

### Key Job Statuses

- **Queued**: Job waiting in queue
- **Processing**: Currently generating invoice
- **Completed**: Invoice ready for download
- **Failed**: An error occurred during processing

## Prerequisites

- .NET 10
- Azure Storage Account with Blob Storage container
- Azure CLI (for local development authentication)

## Setup

### 1. Create Azure Resources

### 2. Configure RBAC

Assign the **Storage Blob Data Owner** role to your user (required for blob tagging):

### 3. Login with Azure CLI

```powershell
az login
```

### 4. Configure Application Settings

Update `appsettings.Development.json`:

```json
{
  "AzureBlobStorage": {
	"Uri": "https://invoicestorage.blob.core.windows.net/invoices"
  }
}
```

### 5. Run the Application

The API will be available at `https://localhost:7194` (check `launchSettings.json` for actual port).

## Usage Example

### Workflow

1. **Generate Invoice**
1. 
2. **Check Status** (poll until completed)

3. **Download Invoice**

## Important Notes

### Blob Tagging Permissions

Blob tagging operations require the **Storage Blob Data Owner** role. If you encounter a `403 Forbidden` error:

```
AuthorizationPermissionMismatch: This operation is not permitted as the blob has tags and you lack permission to modify tags on this blob.
```

Ensure your user/service principal has the correct role assignment.

## Future Enhancements

- [ ] **Azure Entra ID Authentication**: Implement OAuth 2.0 for API security
- [ ] **Authorization (AuthZ)**: Role-based access control for endpoints
- [ ] **Azure Queue Storage**: Replace in-memory Channel with durable Azure Queue
- [ ] **Azure Functions**: Run background job processing via serverless compute
- [ ] **Retry Policies**: Add exponential backoff for transient failures
- [ ] **Webhook Notifications**: Notify clients when invoices complete
- [ ] **Batch Operations**: Support generating multiple invoices in one request
- [ ] **Progress Tracking**: Expose more granular progress details

## Learning Points

This project demonstrates:

1. **Async/Await Patterns**: Non-blocking API operations with background processing
2. **Channel-based Queuing**: Using `System.Threading.Channels` for high-performance queuing
3. **BackgroundService**: Building hosted services for long-running tasks
4. **Dependency Injection**: Structuring services with .NET DI container
5. **Azure Integration**: Authenticating and interacting with Azure services
6. **OpenAPI Integration**: Auto-generating API documentation
7. **Exception Handling**: Middleware-based global error handling
8. **HATEOAS**: Providing resource links in API responses

## Technologies

- **.NET 10**: Latest C# language features
- **ASP.NET Core**: Web framework
- **System.Threading.Channels**: Async-friendly job queue
- **Azure Storage Blobs**: Cloud file storage with tagging
- **Azure.Identity**: Default credential chain for authentication
- **Bogus**: Fake data generation library
- **Handlebars**: Template engine for PDF generation
- **Swagger/OpenAPI**: API documentation
