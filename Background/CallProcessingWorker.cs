using CallManagement.Data;
using CallManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace CallManagement.Background;

public class CallProcessingWorker(
    CallQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<CallProcessingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var callId in queue.ReadAllAsync(stoppingToken))
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            try
            {
                var call = await db.Calls.Include(c => c.Lead)
                    .FirstOrDefaultAsync(c => c.Id == callId, stoppingToken);
                if (call is null) continue;

                var transcription = scope.ServiceProvider.GetRequiredService<ITranscriptionService>();
                var llm = scope.ServiceProvider.GetRequiredService<ILlmClient>();

                call.Transcript = await transcription.TranscribeAsync(call.RecordingUrl, stoppingToken);
                var lead = await llm.ExtractLeadAsync(call.Transcript, stoppingToken);
                lead.CallId = call.Id;
                call.Lead = lead;
                call.Status = "Done";
                call.ErrorMessage = null;
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Call {CallId} processing failed", callId);
                var failedCall = await db.Calls.FirstOrDefaultAsync(c => c.Id == callId, stoppingToken);
                if (failedCall is not null)
                {
                    failedCall.Status = "Failed";
                    failedCall.ErrorMessage = ex.Message;
                    await db.SaveChangesAsync(stoppingToken);
                }
            }
        }
    }
}
