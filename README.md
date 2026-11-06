# Appliance Repair Call CRM

An ASP.NET Core MVC application that receives appliance-repair call recordings, processes them asynchronously, extracts customer and repair details using AI, and displays the results in a browser-based CRM.

## 1. Requirements

- .NET 8 SDK
- SQLite (included through the application configuration)
- OpenAI API key only when using real AI services

## 2. Run the Application

From the project folder:

```bash
dotnet restore
dotnet run
```

Open the localhost URL printed in the terminal.

The application uses SQLite (`calls.db`) and mock AI services by default. No API key is required.

Use the browser dropdown to select a sample and click **Simulate incoming call**. The request is sent to the actual webhook endpoint.

The dashboard refreshes automatically every two seconds.

## 3. Webhook Endpoint

**POST** `/api/calls/recording`

Accepts JSON and form-encoded requests.

The webhook validates the incoming request, saves the call with `Processing` status, adds its ID to the background queue, and returns an HTTP response without performing AI processing.

**GET** `/api/calls`

Returns calls and their extracted lead information for the CRM dashboard.

## 4. Mock Scenarios

The application includes three sample recordings:

- `mock://maria` — Maria Lopez, Samsung refrigerator not cooling, urgent callback.
- `mock://john` — John, Whirlpool washing machine not draining, next week in the morning.
- `mock://unknown` — Unknown caller, oven making an unusual noise, brand not provided.

## 5. Design and Architecture

The application uses dependency injection and interfaces to keep mock and real AI providers interchangeable.

Processing flow:

1. The webhook receives and validates a recording.
2. The call is saved in SQLite with `Processing` status.
3. The call ID is added to an in-memory background queue.
4. A hosted background worker retrieves the call.
5. `ITranscriptionService` converts the recording into text.
6. `ILlmClient` extracts structured customer and appliance information.
7. The result is saved, and the call status becomes `Done` or `Failed`.
8. The browser dashboard retrieves updated information through polling.

## 6. Real OpenAI Configuration

To enable real AI services, configure:

```text
AI:UseMocks = false
AI:OpenAI:ApiKey = YOUR_API_KEY
```

Environment variables can be used:

```powershell
$env:AI__UseMocks = "false"
$env:AI__OpenAI__ApiKey = "YOUR_API_KEY"
dotnet run
```

Alternatively, use .NET user-secrets.

Never commit API keys or other credentials to the repository.

The real transcription implementation downloads the recording and sends it to OpenAI's audio transcription endpoint. The LLM implementation uses Chat Completions with JSON output.

A publicly reachable recording URL is required for the real transcription path.

## 7. Production Improvements

Before production deployment, the following improvements would be recommended:

- Replace the in-memory queue with a durable queue such as Azure Service Bus or RabbitMQ.
- Implement retry policies with exponential backoff.
- Validate webhook signatures.
- Add authentication and authorization.
- Add structured logging, monitoring, and alerting.
- Use database migrations.
- Validate recording URLs and enforce file-size limits.
- Store API keys securely.

The current in-memory queue is intentionally lightweight for this assessment and does not survive an application restart.

## 8. AI Assistance Disclosure

AI assistance was used to draft and review portions of the implementation. The candidate is responsible for reviewing, testing, understanding, and explaining the complete application, including the webhook, background processing, AI integration, error handling, and design decisions.