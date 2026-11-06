using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CallManagement.Models;

namespace CallManagement.Services.Real;

public class OpenAiLlmClient(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : ILlmClient
{
    public async Task<LeadExtractionResult> ExtractLeadAsync(string transcript, CancellationToken cancellationToken = default)
    {
        var apiKey = configuration["AI:OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OpenAI API key is not configured.");

        var prompt = """
        Extract appliance-repair lead details from the transcript.
        Return ONLY valid JSON with these fields:
        customerName (string or null), appliance (fridge|washer|dryer|dishwasher|oven|other),
        brand (string or null), problem (short string), urgency (low|medium|high),
        preferredTime (string or null). Do not include markdown.
        """;

        var payload = new
        {
            model = configuration["AI:OpenAI:ChatModel"] ?? "gpt-4o-mini",
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new { role = "system", content = prompt },
                new { role = "user", content = transcript }
            }
        };

        var client = httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await client.SendAsync(request, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(raw);
        var content = document.RootElement.GetProperty("choices")[0]
            .GetProperty("message").GetProperty("content").GetString();

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("AI returned an empty response.");

        return JsonSerializer.Deserialize<LeadExtractionResult>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new JsonException("AI returned invalid lead JSON.");
    }
}
