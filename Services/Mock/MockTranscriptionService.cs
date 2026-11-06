using CallManagement.Services;

namespace CallManagement.Services.Mock;

public class MockTranscriptionService : ITranscriptionService
{
    public Task<string> TranscribeAsync(string recordingUrl, CancellationToken cancellationToken = default)
    {
        var transcript = recordingUrl.ToLowerInvariant() switch
        {
            "mock://maria" => "Hi, this is Maria Lopez. My Samsung fridge stopped cooling since yesterday, all my food is going bad. Please call me back as soon as possible, I'm home all day.",
            "mock://john" => "Hello, I have a Whirlpool washer, it doesn't drain water at the end of the cycle. No rush, maybe next week in the morning. My name is John.",
            "mock://unknown" => "Yeah hi... uh, the oven, it's making a weird noise, I don't know the brand. Call me.",
            _ => throw new InvalidOperationException("Mock recording not recognized. Use one of the sample recordings.")
        };

        return Task.FromResult(transcript);
    }
}
