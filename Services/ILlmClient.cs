using CallManagement.Models;

namespace CallManagement.Services;

public interface ILlmClient
{
    Task<LeadExtractionResult> ExtractLeadAsync(string transcript, CancellationToken cancellationToken = default);
}
