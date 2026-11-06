using CallManagement.Models;

namespace CallManagement.Services.Mock;

public class MockLlmClient : ILlmClient
{
    public Task<LeadExtractionResult> ExtractLeadAsync(string transcript, CancellationToken cancellationToken = default)
    {
        var text = transcript.ToLowerInvariant();
        var lead = text.Contains("maria")
            ? new LeadExtractionResult { CustomerName = "Maria Lopez", Appliance = "fridge", Brand = "Samsung", Problem = "Fridge stopped cooling; food is spoiling.", Urgency = "high", PreferredTime = "Home all day; call back as soon as possible." }
            : text.Contains("whirlpool") || text.Contains("washer")
                ? new LeadExtractionResult { CustomerName = "John", Appliance = "washer", Brand = "Whirlpool", Problem = "Washer does not drain water at the end of the cycle.", Urgency = "low", PreferredTime = "Next week in the morning." }
                : new LeadExtractionResult { CustomerName = null, Appliance = "oven", Brand = null, Problem = "Oven is making a weird noise.", Urgency = "medium", PreferredTime = null };

        return Task.FromResult(lead);
    }
}
