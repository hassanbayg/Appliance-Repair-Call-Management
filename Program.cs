using CallManagement.Background;
using CallManagement.Data;
using CallManagement.Services;
using CallManagement.Services.Mock;
using CallManagement.Services.Real;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=calls.db"));

builder.Services.AddSingleton<CallQueue>();
builder.Services.AddHostedService<CallProcessingWorker>();

var useMocks = builder.Configuration.GetValue<bool>("AI:UseMocks", true);
if (useMocks)
{
    builder.Services.AddScoped<ITranscriptionService, MockTranscriptionService>();
    builder.Services.AddScoped<ILlmClient, MockLlmClient>();
}
else
{
    builder.Services.AddHttpClient();
    builder.Services.AddScoped<ITranscriptionService, OpenAiTranscriptionService>();
    builder.Services.AddScoped<ILlmClient, OpenAiLlmClient>();
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=CallsPage}/{action=Index}/{id?}");

app.Run();
