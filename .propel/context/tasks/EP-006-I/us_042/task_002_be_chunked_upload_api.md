# Task - task_002_be_chunked_upload_api

## Requirement Reference
- User Story: US_042
- Story Location: .propel/context/tasks/EP-006-I/us_042/us_042.md
- Acceptance Criteria:
    - **AC1**: Given I am on the Document Upload page, When I drag-and-drop or select a PDF file, Then the system validates file format (PDF only) and size (max 10MB) before initiating upload.
    - **AC2**: Given the file is valid, When the upload begins, Then a real-time progress bar updates continuously via Pusher Channels showing percentage complete using chunked upload (TR-022).
    - **AC3**: Given the upload completes successfully, When the file is stored, Then a confirmation message displays with the document name, size, and status "Uploaded — Processing pending".
    - **AC4**: Given the file is invalid, When I select a non-PDF or oversized file, Then inline validation displays "Only PDF files up to 10MB are supported" without initiating the upload.
- Edge Case:
    - What happens when the upload is interrupted (network drop)? Chunked upload allows resume from the last successful chunk; user sees "Upload paused — Retrying..." message.
    - How does the system handle simultaneous uploads of multiple documents? Each upload tracks progress independently with individual progress bars.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | No |
| **Figma URL** | N/A |
| **Wireframe Status** | N/A |
| **Wireframe Type** | N/A |
| **Wireframe Path/URL** | N/A |
| **Screen Spec** | N/A |
| **UXR Requirements** | N/A |
| **Design Tokens** | N/A |

> If UI Impact = No, all design references should be N/A

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Backend | .NET | 8.0 |
| Backend | ASP.NET Core Web API | 8.0 |
| Database | PostgreSQL | 16.x |
| Library | Pusher .NET Server | 5.x |
| Library | Entity Framework Core | 8.0 |
| AI/ML | N/A | N/A |
| Vector Store | N/A | N/A |
| AI Gateway | N/A | N/A |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | No |
| **AIR Requirements** | N/A |
| **AI Pattern** | N/A |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | N/A |
| **Model Provider** | N/A |

> If AI Impact = No, all AI references should be N/A

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview

Implement the backend API for chunked file upload with real-time progress tracking via Pusher Channels. This task creates three RESTful endpoints: Initialize Upload (creates upload session and validates file metadata), Upload Chunk (receives file chunks with sequence tracking and broadcasts progress), and Finalize Upload (completes upload, validates integrity, creates database record). The implementation must support resumable uploads (tracking completed chunks), concurrent multi-user uploads (session isolation), and graceful degradation when Pusher is unavailable (queue events for retry).

**Key Capabilities:**
- Session-based chunked upload with resume capability
- Real-time progress broadcasting via Pusher Channels
- File validation (PDF MIME type, max 10MB) before accepting upload
- Chunk integrity verification (hash validation, sequence tracking)
- Temporary storage during upload, permanent storage on finalization
- Concurrent upload support with session isolation
- Comprehensive error handling and logging
- Background cleanup of abandoned upload sessions

## Dependent Tasks
- task_003_db_document_schema_migration (must complete first to create ClinicalDocument table)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Web/Controllers/DocumentsController.cs` - API controller with 3 endpoints
- **NEW**: `src/backend/PatientAccess.Business/Services/DocumentUploadService.cs` - Core upload logic
- **NEW**: `src/backend/PatientAccess.Business/Services/ChunkedUploadManager.cs` - Session and chunk management
- **NEW**: `src/backend/PatientAccess.Business/DTOs/InitializeUploadRequestDto.cs` - Initialize upload request
- **NEW**: `src/backend/PatientAccess.Business/DTOs/InitializeUploadResponseDto.cs` - Upload session metadata
- **NEW**: `src/backend/PatientAccess.Business/DTOs/UploadChunkRequestDto.cs` - Chunk upload request
- **NEW**: `src/backend/PatientAccess.Business/DTOs/ChunkUploadResponseDto.cs` - Chunk status response
- **NEW**: `src/backend/PatientAccess.Business/DTOs/FinalizeUploadRequestDto.cs` - Finalize upload request
- **NEW**: `src/backend/PatientAccess.Business/DTOs/DocumentUploadResponseDto.cs` - Final document metadata
- **NEW**: `src/backend/PatientAccess.Business/BackgroundJobs/UploadSessionCleanupJob.cs` - Cleanup abandoned sessions
- **MODIFY**: `src/backend/PatientAccess.Business/Services/PusherService.cs` - Add upload progress event methods
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register new services in DI container

## Implementation Plan

1. **Create DTOs for Upload Operations**
   - Define `InitializeUploadRequestDto` with fileName, fileSize, mimeType, totalChunks
   - Define `InitializeUploadResponseDto` with uploadSessionId, chunkSize, expiresAt
   - Define `UploadChunkRequestDto` with uploadSessionId, chunkIndex, chunkData (IFormFile)
   - Define `ChunkUploadResponseDto` with chunksReceived, percentComplete, status
   - Define `FinalizeUploadRequestDto` with uploadSessionId
   - Define `DocumentUploadResponseDto` with documentId, fileName, fileSize, status, uploadedAt
   - Follow existing DTO patterns in `PatientAccess.Business/DTOs/`

2. **Implement ChunkedUploadManager Service**
   - Create upload session in-memory cache (ConcurrentDictionary) with session metadata
   - Track received chunks per session (List<int> completedChunkIndices)
   - Implement `CreateSession(fileName, fileSize, totalChunks, userId)` → returns session ID (GUID)
   - Implement `SaveChunk(sessionId, chunkIndex, chunkData)` → saves to temp storage, returns progress
   - Implement `IsSessionComplete(sessionId)` → checks if all chunks received
   - Implement `FinalizeSession(sessionId)` → assembles chunks into final file, returns file path
   - Implement `CleanupSession(sessionId)` → deletes temp chunks and session metadata
   - Use `IMemoryCache` for session storage with 1-hour sliding expiration

3. **Implement DocumentUploadService**
   - Inject `ChunkedUploadManager`, `PusherService`, `ApplicationDbContext`, `ILogger`
   - Implement `InitializeUploadAsync(request, userId)` → validates file metadata, creates session
   - Validation: Check MIME type = "application/pdf", fileSize <= 10MB (10485760 bytes)
   - Implement `UploadChunkAsync(request, userId)` → delegates to ChunkedUploadManager
   - Trigger Pusher event `chunk-uploaded` with progress percentage after each chunk
   - Implement `FinalizeUploadAsync(request, userId)` → assembles file, creates DB record, moves to permanent storage
   - Create `ClinicalDocument` entity with status "Uploaded", processingStatus "Processing"
   - Trigger Pusher event `upload-complete` with document metadata
   - Handle errors gracefully: cleanup session on failure, trigger `upload-failed` event

4. **Create DocumentsController**
   - Add `[ApiController]`, `[Route("api/documents")]` attributes
   - Inject `DocumentUploadService`, `ILogger`
   - Add `[Authorize]` attribute (require authenticated users)
   - **Endpoint 1**: `POST /api/documents/upload/initialize`
     - Accepts `InitializeUploadRequestDto` from request body
     - Returns `201 Created` with `InitializeUploadResponseDto`
     - Returns `400 Bad Request` for invalid file metadata (non-PDF, oversized)
   - **Endpoint 2**: `POST /api/documents/upload/chunk`
     - Accepts `[FromForm] UploadChunkRequestDto` (multipart/form-data)
     - Returns `200 OK` with `ChunkUploadResponseDto`
     - Returns `404 Not Found` if session expired or invalid
   - **Endpoint 3**: `POST /api/documents/upload/finalize`
     - Accepts `FinalizeUploadRequestDto` from request body
     - Returns `200 OK` with `DocumentUploadResponseDto`
     - Returns `400 Bad Request` if upload incomplete or session invalid
   - Follow existing controller patterns from `AppointmentsController.cs`

5. **Enhance PusherService for Upload Events**
   - Add method `TriggerChunkUploadedAsync(uploadSessionId, percentComplete, chunksReceived, totalChunks)`
   - Add method `TriggerUploadCompleteAsync(uploadSessionId, documentId, fileName, fileSize)`
   - Add method `TriggerUploadFailedAsync(uploadSessionId, errorMessage)`
   - Use channel name pattern: `document-upload-{uploadSessionId}`
   - Implement graceful degradation (log event if Pusher unavailable, don't fail upload)

6. **Implement Upload Session Cleanup Job**
   - Create `UploadSessionCleanupJob` using Hangfire pattern (reference `ConfirmationEmailJob.cs`)
   - Schedule job to run every 30 minutes
   - Query in-memory cache for expired sessions (expiresAt < DateTime.UtcNow)
   - Delete temp chunk files from storage
   - Remove session from cache
   - Log cleanup activity with session count

7. **Configure Dependency Injection**
   - Register `ChunkedUploadManager` as Singleton in `Program.cs`
   - Register `DocumentUploadService` as Scoped in `Program.cs`
   - Configure Hangfire job for `UploadSessionCleanupJob`
   - Verify `PusherService` already registered from existing code
   - Add file storage path configuration in `appsettings.json` (uploadTempPath, uploadPermanentPath)

8. **Implement File Storage Strategy**
   - Temp storage: `{uploadTempPath}/{sessionId}/chunk_{index}.tmp`
   - Permanent storage: `{uploadPermanentPath}/{userId}/{documentId}.pdf`
   - Use `Directory.CreateDirectory` to ensure paths exist
   - Use `FileStream` for chunk writing (append mode)
   - Use `File.Move` for final assembly (atomic operation)
   - Implement cleanup on errors (delete temp files)

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── DTOs/
│   │   ├── AppointmentResponseDto.cs
│   │   ├── CreateAppointmentRequestDto.cs
│   │   ├── CreateUserRequestDto.cs
│   │   └── LoginRequestDto.cs
│   ├── Services/
│   │   ├── AppointmentService.cs
│   │   ├── AuthService.cs
│   │   ├── PdfGenerationService.cs
│   │   ├── PusherService.cs
│   │   └── WaitlistService.cs
│   ├── BackgroundJobs/
│   │   ├── ConfirmationEmailJob.cs
│   │   └── SlotAvailabilityMonitor.cs
│   └── Interfaces/
│       └── IAppointmentService.cs
├── PatientAccess.Web/
│   ├── Controllers/
│   │   ├── AppointmentsController.cs
│   │   ├── AuthController.cs
│   │   ├── ProvidersController.cs
│   │   └── WaitlistController.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── appsettings.Development.json
└── PatientAccess.Tests/
    └── Services/
        └── AppointmentServiceTests.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Web/Controllers/DocumentsController.cs | API controller with Initialize, UploadChunk, Finalize endpoints |
| CREATE | src/backend/PatientAccess.Business/Services/DocumentUploadService.cs | Core upload orchestration logic |
| CREATE | src/backend/PatientAccess.Business/Services/ChunkedUploadManager.cs | Session and chunk management service |
| CREATE | src/backend/PatientAccess.Business/DTOs/InitializeUploadRequestDto.cs | Initialize upload request DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/InitializeUploadResponseDto.cs | Upload session response DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/UploadChunkRequestDto.cs | Chunk upload request DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/ChunkUploadResponseDto.cs | Chunk status response DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/FinalizeUploadRequestDto.cs | Finalize request DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/DocumentUploadResponseDto.cs | Final document metadata DTO |
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/UploadSessionCleanupJob.cs | Cleanup job for abandoned sessions |
| MODIFY | src/backend/PatientAccess.Business/Services/PusherService.cs | Add upload progress event methods |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register new services and configure storage paths |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add file storage configuration |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### .NET File Handling Documentation
- **IFormFile Upload**: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads
- **FileStream Operations**: https://learn.microsoft.com/en-us/dotnet/api/system.io.filestream
- **Directory Management**: https://learn.microsoft.com/en-us/dotnet/api/system.io.directory

### ASP.NET Core Best Practices
- **Chunked Upload Pattern**: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads#upload-large-files-with-streaming
- **Multipart Form Data**: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads#upload-files-with-buffering
- **API Controller Patterns**: https://learn.microsoft.com/en-us/aspnet/core/web-api/

### Pusher .NET Server SDK
- **Pusher .NET Documentation**: https://pusher.com/docs/channels/server_api/dotnet/
- **Triggering Events**: https://pusher.com/docs/channels/server_api/dotnet/#triggering-events
- **Channel Naming**: https://pusher.com/docs/channels/using_channels/channels/#channel-naming

### Memory Cache
- **IMemoryCache**: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory
- **ConcurrentDictionary**: https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2

### Existing Codebase Patterns
- **Controller Pattern**: `src/backend/PatientAccess.Web/Controllers/AppointmentsController.cs`
- **Service Pattern**: `src/backend/PatientAccess.Business/Services/AppointmentService.cs`
- **Pusher Service Pattern**: `src/backend/PatientAccess.Business/Services/PusherService.cs`
- **Background Job Pattern**: `src/backend/PatientAccess.Business/BackgroundJobs/ConfirmationEmailJob.cs`
- **DTO Pattern**: `src/backend/PatientAccess.Business/DTOs/CreateAppointmentRequestDto.cs`

## Build Commands
```powershell
# Navigate to backend directory
cd src/backend

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run application
cd PatientAccess.Web
dotnet run
```

## Implementation Validation Strategy
- [ ] Unit tests pass (upload session management, chunk assembly, validation logic)
- [ ] Integration tests pass (end-to-end upload flow, Pusher event delivery)
- [ ] API endpoints return correct status codes (201, 200, 400, 404)
- [ ] File validation enforces PDF only and max 10MB
- [ ] Chunked upload resumes correctly after interruption
- [ ] Multiple concurrent uploads isolated by session ID
- [ ] Pusher events broadcast correctly (test with real Pusher credentials)
- [ ] Cleanup job removes expired sessions successfully
- [ ] Error handling returns actionable messages
- [ ] File storage paths created and permissions validated

## Implementation Checklist
- [ ] Create all DTOs for upload operations (Initialize, Chunk, Finalize, Response)
- [ ] Implement ChunkedUploadManager with session cache, chunk tracking, file assembly
- [ ] Implement DocumentUploadService with validation, Pusher integration, DB persistence
- [ ] Create DocumentsController with 3 endpoints (Initialize, UploadChunk, Finalize)
- [ ] Enhance PusherService with upload event methods (chunk-uploaded, upload-complete, upload-failed)
- [ ] Implement UploadSessionCleanupJob for abandoned session cleanup
- [ ] Configure DI container in Program.cs (register services, configure storage paths)
- [ ] Add file storage configuration to appsettings.json (temp and permanent paths)
