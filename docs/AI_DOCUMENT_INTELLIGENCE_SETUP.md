# AI Document Intelligence Setup Guide

This guide explains how to set up and use Tesseract OCR and Google Gemini AI for clinical document data extraction.

## Overview

The document intelligence pipeline consists of three stages:

1. **Document Storage** - Supabase Storage for secure PDF storage
2. **OCR Extraction** - Tesseract for text extraction from PDFs
3. **AI Extraction** - Google Gemini for structured clinical data extraction

## Prerequisites

### 1. Tesseract Setup

Tesseract requires trained data files for OCR to work.

**Packages Used:**
- `Tesseract` (v5.2.0) - OCR engine wrapper
- `Docnet.Core` (v2.6.0) - PDF to image conversion  
- `System.Drawing.Common` (v8.0.0) - Image processing

#### Download Tesseract Language Data

1. Visit: https://github.com/tesseract-ocr/tessdata
2. Download `eng.traineddata` (English language pack)
3. Place it in: `src/backend/PatientAccess.Web/tessdata/eng.traineddata`

```powershell
# Create directory
New-Item -ItemType Directory -Force -Path "src/backend/PatientAccess.Web/tessdata"

# Download English trained data (PowerShell)
Invoke-WebRequest -Uri "https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata" `
  -OutFile "src/backend/PatientAccess.Web/tessdata/eng.traineddata"
```

#### Alternative: Fast Language Pack

For faster (but slightly less accurate) OCR, use the fast language pack:

```powershell
Invoke-WebRequest -Uri "https://github.com/tesseract-ocr/tessdata_fast/raw/main/eng.traineddata" `
  -OutFile "src/backend/PatientAccess.Web/tessdata/eng.traineddata"
```

### 2. Google Gemini API Setup

**Package Used:**
- `Mscc.GenerativeAI` (v2.2.0) - Google Gemini .NET SDK

#### Get API Key

1. Visit: https://aistudio.google.com/apikey
2. Sign in with your Google account
3. Click "Create API Key"
4. Copy the generated API key

#### Configure API Key

**Development (appsettings.Development.json)**:
```json
{
  "GeminiAI": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

**Production (Environment Variable)**:
```bash
# Linux/Mac
export GEMINIAI__APIKEY="your-api-key-here"

# Windows PowerShell
$env:GEMINIAI__APIKEY="your-api-key-here"

# Windows Command Prompt
set GEMINIAI__APIKEY=your-api-key-here
```

#### API Rate Limits (Free Tier)

- **15 requests per minute (RPM)**
- **1 million tokens per minute (TPM)**
- **1,500 requests per day (RPD)**

The implementation includes automatic fallback to stub data if rate limits are exceeded.

## Configuration

### appsettings.json

```json
{
  "Tesseract": {
    "DataPath": "./tessdata",
    "Language": "eng"
  },
  "GeminiAI": {
    "ApiKey": "SET_VIA_ENV_GEMINIAI__APIKEY",
    "ModelName": "gemini-1.5-flash-latest",
    "MaxTokens": 8000
  }
}
```

### Configuration Options

#### Tesseract Options

- **DataPath**: Path to tessdata directory (default: `./tessdata`)
- **Language**: Language pack to use (default: `eng`)
  - Additional languages: `fra` (French), `spa` (Spanish), `deu` (German), etc.

#### Gemini Options

- **ApiKey**: Your Google AI Studio API key
- **ModelName**: Gemini model to use
  - `gemini-1.5-flash-latest` - Fast, cost-effective (recommended)
  - `gemini-1.5-pro-latest` - More capable, slower, higher cost
- **MaxTokens**: Maximum tokens per request (default: 8000)

## Usage

### Extract Data from Document

```csharp
// Inject the service
private readonly IClinicalDataExtractionService _extractionService;

// Extract clinical data
var result = await _extractionService.ExtractClinicalDataAsync(documentId);

// Check results
Console.WriteLine($"Extracted {result.TotalDataPoints} data points");
Console.WriteLine($"Requires review: {result.RequiresManualReview}");

foreach (var dataPoint in result.DataPoints)
{
    Console.WriteLine($"{dataPoint.DataType}: {dataPoint.DataKey} = {dataPoint.DataValue}");
    Console.WriteLine($"  Confidence: {dataPoint.ConfidenceScore:F2}%");
}
```

### Direct OCR Usage

```csharp
// Inject Tesseract service
private readonly ITesseractOcrService _ocrService;

// Perform OCR
var ocrResults = await _ocrService.ExtractTextFromPdfAsync("path/to/document.pdf");

foreach (var page in ocrResults)
{
    Console.WriteLine($"Page {page.PageNumber}: {page.ExtractedText}");
    Console.WriteLine($"Confidence: {page.ConfidenceScore:F2}%");
}
```

### Direct Gemini Usage

```csharp
// Inject Gemini service
private readonly IGeminiAiService _geminiService;

// Load prompt template
var promptTemplate = File.ReadAllText(".propel/prompts/clinical-data-extraction.md");

// Extract data
var dataPoints = await _geminiService.ExtractClinicalDataAsync(ocrText, promptTemplate);

foreach (var point in dataPoints)
{
    Console.WriteLine($"{point.DataKey}: {point.DataValue}");
}
```

## Troubleshooting

### Tesseract Issues

**Error: "Tesseract language file not found"**
- Download `eng.traineddata` as described above
- Verify the file is in the correct location

**Error: "Failed to process page"**
- Check PDF is not corrupted
- Ensure PDF is not password-protected
- Verify sufficient disk space for temporary images

### Gemini Issues

**Error: "Gemini API key not configured"**
- Set the `GEMINIAI__APIKEY` environment variable
- Or add ApiKey to appsettings.Development.json

**Warning: "Gemini model not initialized"**
- Check API key is valid
- Verify internet connectivity
- Check Google AI Studio service status

**Error: "Rate limit exceeded"**
- Free tier: 15 requests/minute, 1500 requests/day
- Implement request queuing or upgrade to paid tier
- Service will fall back to stub data automatically

**Error: "Failed to parse Gemini response"**
- Check prompt template is correctly formatted
- Verify model is returning valid JSON
- Enable debug logging to see raw response

## Performance Considerations

### OCR Performance

- **Resolution**: Uses 300 DPI for good quality/speed balance
- **Processing Time**: ~2-5 seconds per page
- **Memory**: ~100MB per page (temporary)
- **PDF Optimization**: Consider pre-processing PDFs to reduce size

### Gemini Performance

- **Processing Time**: ~1-3 seconds per page
- **Token Usage**: ~1000-3000 tokens per page
- **Concurrent Requests**: Respect 15 RPM limit
- **Caching**: Consider caching extracted data

### Overall Pipeline

- **Target**: <30 seconds for typical 5-page document
- **Warning**: Logs warning if extraction exceeds 30s
- **Optimization**: Process pages in parallel (future enhancement)

## Testing

### Manual Testing

1. Place a test PDF in `src/backend/PatientAccess.Web/pdfs/test.pdf`
2. Start the application
3. Upload document via `/api/documents/upload` endpoint
4. Trigger extraction via `/api/clinical-extraction/{documentId}`

### Unit Testing

See `PatientAccess.Tests` project for unit tests:
- `TesseractOcrServiceTests.cs`
- `GeminiAiServiceTests.cs`
- `ClinicalDataExtractionServiceTests.cs`

## Security

- **API Keys**: Never commit API keys to source control
- **Environment Variables**: Use environment variables in production
- **Access Control**: Restrict document access to authorized users only
- **Data Sanitization**: OCR text is sanitized before AI processing
- **Audit Logging**: All AI interactions are logged

## Cost Estimation

### Free Tier (Gemini)

- **Up to 1,500 requests/day** - Free
- **Typical document**: 5 pages = 5 requests = ~0.33% of daily quota
- **Daily capacity**: ~300 documents/day on free tier

### Tesseract

- **Completely free** - Open source
- **No API limits** - Runs locally
- **Cost**: Server compute time only

## Additional Resources

- [Tesseract Documentation](https://github.com/tesseract-ocr/tesseract)
- [Tesseract Language Data](https://github.com/tesseract-ocr/tessdata)
- [Google Gemini API Documentation](https://ai.google.dev/docs)
- [Google AI Studio](https://aistudio.google.com/)
- [Google.GenerativeAI .NET SDK](https://github.com/google-gemini/generative-ai-dotnet)

## Support

For issues or questions:
1. Check this documentation
2. Review application logs
3. Consult team documentation in `.propel/context/`
4. Contact the development team
