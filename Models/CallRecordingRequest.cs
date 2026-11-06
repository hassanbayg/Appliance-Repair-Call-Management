using System.Text.Json.Serialization;

namespace CallManagement.Models;

public class CallRecordingRequest
{
    public string CallSid { get; set; } = "";
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public string RecordingSid { get; set; } = "";
    public string RecordingUrl { get; set; } = "";

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int RecordingDuration { get; set; }
}