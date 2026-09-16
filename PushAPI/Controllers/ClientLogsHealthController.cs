using Microsoft.AspNetCore.Mvc;
using SharedLib.DTO;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

namespace PushAPI.Controllers;

[ApiController]
[Route("api/clientlogs")]
public class ClientLogsHealthController(IWebHostEnvironment env) : ControllerBase
{
    private const string OfflineMessage = "Log uploader offline / upload failed";

    [HttpGet("health")]
    public IActionResult Health([FromQuery] int days = 7, [FromQuery] int offlineSessionGapMinutes = 5)
    {
        try
        {
            var root = Path.Combine(env.ContentRootPath, "App_Data", "ClientLogs");
            if (!Directory.Exists(root))
                return Content("<h2>No log directory found.</h2>", "text/html; charset=utf-8");

            var files = Directory.GetFiles(root, "*.ndjson", SearchOption.TopDirectoryOnly);
            var events = LoadEvents(files);

            var fromUtc = DateTime.UtcNow.Date.AddDays(-Math.Max(1, days) + 1);
            events = events.Where(e => e.UtcTime >= fromUtc).OrderBy(e => e.UtcTime).ToList();

            var summary = events
                .GroupBy(e => new { e.Device, Day = e.UtcTime.ToString("yyyy-MM-dd") })
                .Select(g => new SummaryRow(
                    g.Key.Device,
                    g.Key.Day,
                    g.Count(),
                    g.Count(x => x.Level == "Error"),
                    g.Count(x => x.Level == "Warn"),
                    g.Count(x => x.Message == OfflineMessage),
                    g.Select(x => x.Message).Distinct().Count()))
                .OrderBy(x => x.Day).ThenBy(x => x.Device)
                .ToList();

            var topErrors = events
                .Where(e => e.Level == "Error")
                .GroupBy(e => e.Message)
                .Select(g => new TopErrorRow(g.Key, g.Count()))
                .OrderByDescending(x => x.Count)
                .Take(20)
                .ToList();

            var offlineSessions = BuildOfflineSessions(events, offlineSessionGapMinutes);

            var topActions = events
                .Where(e => e.Level == "Info")
                .GroupBy(e => new { e.Device, e.Message })
                .Select(g => new TopActionRow(g.Key.Device, g.Key.Message, g.Count()))
                .OrderByDescending(x => x.Count)
                .Take(50)
                .ToList();

            var html = BuildHtml(days, offlineSessionGapMinutes, summary, topErrors, offlineSessions, topActions);
            return Content(html, "text/html; charset=utf-8");
        }
        catch (Exception ex)
        {
            var msg = WebUtility.HtmlEncode(ex.ToString());
            return Content($"<h2>Health page error</h2><pre>{msg}</pre>", "text/html; charset=utf-8");
        }
    }

    private static List<LogEvent> LoadEvents(IEnumerable<string> files)
    {
        var result = new List<LogEvent>();

        foreach (var file in files)
        {
            var fileNameNoExt = Path.GetFileNameWithoutExtension(file);
            var device = ExtractDevice(fileNameNoExt);

            foreach (var line in System.IO.File.ReadLines(file))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var dto = JsonSerializer.Deserialize<ClientLogEntryDto>(line);
                    if (dto is null) continue;

                    result.Add(new LogEvent(
                        dto.UtcTime,
                        dto.Level ?? string.Empty,
                        dto.Message ?? string.Empty,
                        dto.Exception,
                        device
                    ));
                }
                catch
                {
                    // Ignore malformed lines
                }
            }
        }

        return result;
    }

    private static string ExtractDevice(string fileNameNoExt)
    {
        // Expected: yyyy-MM-dd_device
        if (fileNameNoExt.Length > 11 && fileNameNoExt[10] == '_')
            return fileNameNoExt[11..];

        return "unknown";
    }

    private static List<OfflineSession> BuildOfflineSessions(List<LogEvent> events, int gapMinutes)
    {
        var gap = TimeSpan.FromMinutes(Math.Max(1, gapMinutes));
        var output = new List<OfflineSession>();

        foreach (var grp in events.Where(e => e.Message == OfflineMessage).GroupBy(e => e.Device))
        {
            var arr = grp.OrderBy(x => x.UtcTime).ToList();
            if (arr.Count == 0) continue;

            var start = arr[0].UtcTime;
            var prev = arr[0].UtcTime;
            var count = 1;

            for (int i = 1; i < arr.Count; i++)
            {
                var cur = arr[i].UtcTime;
                if (cur - prev <= gap)
                {
                    count++;
                    prev = cur;
                    continue;
                }

                output.Add(new OfflineSession(grp.Key, start, prev, (prev - start).TotalMinutes, count));
                start = cur;
                prev = cur;
                count = 1;
            }

            output.Add(new OfflineSession(grp.Key, start, prev, (prev - start).TotalMinutes, count));
        }

        return output.OrderBy(x => x.StartUtc).ToList();
    }

    private static string BuildHtml(
        int days,
        int gapMinutes,
        List<SummaryRow> summary,
        List<TopErrorRow> topErrors,
        List<OfflineSession> offlineSessions,
        List<TopActionRow> topActions)
    {
        var sb = new StringBuilder();

        sb.Append("""
<!doctype html>
<html>
<head>
<meta charset="utf-8" />
<title>Client Logs Health</title>
<style>
body { font-family: Segoe UI, Arial, sans-serif; margin: 20px; }
h1,h2 { margin: 8px 0; }
small { color: #666; }
table { border-collapse: collapse; width: 100%; margin: 8px 0 20px; }
th, td { border: 1px solid #ddd; padding: 6px 8px; font-size: 13px; text-align: left; }
th { background: #f3f3f3; }
.bad { color: #b00020; font-weight: 600; }
.warn { color: #9c6b00; font-weight: 600; }
</style>
</head>
<body>
""");

        sb.Append($"<h1>Client Logs Health</h1><small>UTC | days={days}, offlineSessionGapMinutes={gapMinutes}</small>");

        sb.Append("<h2>1) Device/Day summary</h2><table><tr><th>Device</th><th>Day</th><th>Total</th><th>Errors</th><th>Warnings</th><th>OfflineWarns</th><th>UniqueMessages</th></tr>");
        foreach (var x in summary)
        {
            sb.Append($"<tr><td>{H(x.Device)}</td><td>{H(x.Day)}</td><td>{x.Total}</td><td class='bad'>{x.Errors}</td><td class='warn'>{x.Warnings}</td><td>{x.OfflineWarns}</td><td>{x.UniqueMessages}</td></tr>");
        }
        sb.Append("</table>");

        sb.Append("<h2>2) Top error messages</h2><table><tr><th>Message</th><th>Count</th></tr>");
        foreach (var x in topErrors)
        {
            sb.Append($"<tr><td>{H(x.Message)}</td><td>{x.Count}</td></tr>");
        }
        sb.Append("</table>");

        sb.Append("<h2>3) Offline sessions</h2><table><tr><th>Device</th><th>StartUtc</th><th>EndUtc</th><th>ApproxMinutes</th><th>OfflineRecords</th></tr>");
        foreach (var s in offlineSessions)
        {
            sb.Append($"<tr><td>{H(s.Device)}</td><td>{s.StartUtc:yyyy-MM-dd HH:mm:ss}</td><td>{s.EndUtc:yyyy-MM-dd HH:mm:ss}</td><td>{s.ApproxMinutes.ToString("0.##", CultureInfo.InvariantCulture)}</td><td>{s.OfflineRecords}</td></tr>");
        }
        sb.Append("</table>");

        sb.Append("<h2>4) User action volume (Info)</h2><table><tr><th>Device</th><th>Action</th><th>Count</th></tr>");
        foreach (var x in topActions)
        {
            sb.Append($"<tr><td>{H(x.Device)}</td><td>{H(x.Action)}</td><td>{x.Count}</td></tr>");
        }
        sb.Append("</table>");

        sb.Append("</body></html>");
        return sb.ToString();
    }

    private static string H(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private sealed record LogEvent(DateTime UtcTime, string Level, string Message, string? Exception, string Device);
    private sealed record OfflineSession(string Device, DateTime StartUtc, DateTime EndUtc, double ApproxMinutes, int OfflineRecords);
    private sealed record SummaryRow(
        string Device,
        string Day,
        int Total,
        int Errors,
        int Warnings,
        int OfflineWarns,
        int UniqueMessages);

    private sealed record TopErrorRow(string Message, int Count);

    private sealed record TopActionRow(string Device, string Action, int Count);
}