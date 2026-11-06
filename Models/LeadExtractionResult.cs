namespace CallManagement.Models;

public class LeadExtractionResult
{
    public int Id { get; set; }
    public string? CustomerName { get; set; }
    public string Appliance { get; set; } = "other";
    public string? Brand { get; set; }
    public string Problem { get; set; } = "";
    public string Urgency { get; set; } = "medium";
    public string? PreferredTime { get; set; }
    public int CallId { get; set; }
}
