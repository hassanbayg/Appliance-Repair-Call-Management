
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CallManagement.Services.Real;

public class OpenAiTranscriptionService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : ITranscriptionService
{
    public async Task<string> TranscribeAsync(
        string recordingUrl,
        CancellationToken cancellationToken = default)
    {
        var apiKey = configuration["AI:OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException(
                "OpenAI API key is not configured.");

        if (!Uri.TryCreate(recordingUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp &&
             uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "A valid HTTP or HTTPS recording URL is required for real transcription.");
        }

        var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(60);

        using var audioResponse =
            await client.GetAsync(uri, cancellationToken);

        audioResponse.EnsureSuccessStatusCode();

        var bytes =
            await audioResponse.Content.ReadAsByteArrayAsync(cancellationToken);

        if (bytes.Length == 0)
            throw new InvalidOperationException("Recording audio is empty.");

        using var form = new MultipartFormDataContent();
        using var audio = new ByteArrayContent(bytes);

        audio.Headers.ContentType =
            new MediaTypeHeaderValue("audio/mpeg");

        form.Add(audio, "file", "recording.mp3");
        form.Add(
            new StringContent(
                configuration["AI:OpenAI:TranscriptionModel"] ?? "whisper-1"),
            "model");

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.openai.com/v1/audio/transcriptions");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        request.Content = form;

        using var response =
            await client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<TranscriptionResponse>(
                cancellationToken: cancellationToken);

        return result?.Text ??
            throw new InvalidOperationException(
                "Transcription response was empty.");
    }

    private sealed class TranscriptionResponse
    {
        public string? Text { get; set; }
    }
}
