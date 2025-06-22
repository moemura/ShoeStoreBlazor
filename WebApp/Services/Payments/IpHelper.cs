using Microsoft.AspNetCore.Http;

namespace WebApp.Services.Payments;

public static class IpHelper
{
    public static string GetIpAddress(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        
        // Check for forwarded IP (when behind proxy/load balancer)
        if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim();
        }
        else if (context.Request.Headers.ContainsKey("X-Real-IP"))
        {
            ipAddress = context.Request.Headers["X-Real-IP"];
        }
        
        return !string.IsNullOrEmpty(ipAddress) ? ipAddress : "127.0.0.1";
    }
} 