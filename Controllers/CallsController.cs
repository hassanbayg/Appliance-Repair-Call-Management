using System.Text.Json;
using CallManagement.Background;
using CallManagement.Data;
using CallManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CallManagement.Controllers;

[ApiController]
[Route("api/calls")]
public class CallsController(
    ApplicationDbContext db,
    CallQueue queue) : ControllerBase
{
    [HttpPost("recording")]
    [Consumes("application/json", "application/x-www-form-urlencoded", "multipart/form-data")]
    public async Task<IActionResult> ReceiveRecording(CancellationToken cancellationToken)
    {
        CallRecordingRequest? request;
        if (Request.HasFormContentType)
        {
            var form = await Request.ReadFormAsync(cancellationToken);
            request = new CallRecordingRequest
            {
                CallSid = form["CallSid"].ToString(),
                From = form["From"].ToString(),
                To = form["To"].ToString(),
                RecordingSid = form["RecordingSid"].ToString(),
                RecordingUrl = form["RecordingUrl"].ToString(),
                RecordingDuration = int.TryParse(form["RecordingDuration"], out var duration) ? duration : 0
            };
        }
        else
        {
            request = await JsonSerializer.DeserializeAsync<CallRecordingRequest>(
                Request.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);
        }

        if (request is null || string.IsNullOrWhiteSpace(request.CallSid))
            return BadRequest(new { error = "CallSid is required." });

        var call = new Call
        {
            CallSid = request.CallSid,
            From = request.From,
            To = request.To,
            RecordingSid = request.RecordingSid,
            RecordingUrl = request.RecordingUrl,
            RecordingDuration = request.RecordingDuration,
            Status = "Processing",
            ReceivedAt = DateTime.UtcNow
        };

        db.Calls.Add(call);
        await db.SaveChangesAsync(cancellationToken);
        await queue.EnqueueAsync(call.Id, cancellationToken);

        return Ok(new { callId = call.Id, status = call.Status, message = "Call accepted for processing." });
    }

    [HttpGet]
    public async Task<IActionResult> GetCalls(CancellationToken cancellationToken)
    {
        var calls = await db.Calls.Include(c => c.Lead)
            .OrderByDescending(c => c.ReceivedAt)
            .Select(c => new
            {
                c.Id, c.From, c.Status, c.ErrorMessage, c.ReceivedAt, c.Transcript,
                CustomerName = c.Lead == null ? null : c.Lead.CustomerName,
                Appliance = c.Lead == null ? null : c.Lead.Appliance,
                Brand = c.Lead == null ? null : c.Lead.Brand,
                Problem = c.Lead == null ? null : c.Lead.Problem,
                Urgency = c.Lead == null ? null : c.Lead.Urgency,
                PreferredTime = c.Lead == null ? null : c.Lead.PreferredTime
            }).ToListAsync(cancellationToken);

        return Ok(calls);
    }
}
