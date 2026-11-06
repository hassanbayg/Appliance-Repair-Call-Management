namespace CallManagement.Services;

public interface ITranscriptionService
{
    Task<string> TranscribeAsync(string recordingUrl, CancellationToken cancellationToken = default);
}
