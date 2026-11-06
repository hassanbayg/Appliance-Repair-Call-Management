namespace CallManagement.Models;

public class Call
{
    public int Id { get; set; }
    public string CallSid { get; set; } = "";
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public string RecordingSid { get; set; } = "";
    public string RecordingUrl { get; set; } = "";
    public int RecordingDuration { get; set; }
    public string Status { get; set; } = "Processing";
    public string? ErrorMessage { get; set; }
    public string? Transcript { get; set; }
    public LeadExtractionResult? Lead { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
