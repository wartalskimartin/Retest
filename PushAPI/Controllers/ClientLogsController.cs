using Microsoft.AspNetCore.Mvc;
using SharedLib.DTO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PushAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientLogsController(IWebHostEnvironment env) : ControllerBase
{
    private static readonly SemaphoreSlim FileLock = new(1, 1);

    [HttpPost("batch")]
    public async Task<IActionResult> Batch([FromBody] ClientLogBatchDto batch)
    {
        if (batch.Items.Count == 0) return Ok();

        var root = Path.Combine(env.ContentRootPath, "App_Data", "ClientLogs");
        Directory.CreateDirectory(root);

        await FileLock.WaitAsync();
        try
        {
            var device = Safe(batch.DeviceId);
            foreach (var dayGroup in batch.Items.GroupBy(x => x.UtcTime.Date))
            {
                var file = Path.Combine(root, $"{dayGroup.Key:yyyy-MM-dd}_{device}.ndjson");
                await using var fs = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.Read);
                await using var writer = new StreamWriter(fs);

                foreach (var item in dayGroup)
                    await writer.WriteLineAsync(JsonSerializer.Serialize(item));
            }
        }
        finally
        {
            FileLock.Release();
        }

        return Ok();
    }

    private static string Safe(string value) =>
        Regex.Replace(string.IsNullOrWhiteSpace(value) ? "unknown" : value, @"[^\w\-\.]", "_");
}