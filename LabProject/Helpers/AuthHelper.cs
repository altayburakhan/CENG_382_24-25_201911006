using Microsoft.AspNetCore.Http;

namespace LabProject.Helpers
{
    public static class AuthHelper
    {
        public static bool IsAuthenticated(HttpContext context)
        {
            // Check if session exists
            if (context.Session == null)
                return false;

            // Get values from session
            string sessionUsername = context.Session.GetString("username");
            string sessionToken = context.Session.GetString("token");
            string sessionId = context.Session.GetString("session_id");

            // Check if session values exist
            if (string.IsNullOrEmpty(sessionUsername) || 
                string.IsNullOrEmpty(sessionToken) || 
                string.IsNullOrEmpty(sessionId))
                return false;

            // Get values from cookies
            context.Request.Cookies.TryGetValue("username", out string cookieUsername);
            context.Request.Cookies.TryGetValue("token", out string cookieToken);
            context.Request.Cookies.TryGetValue("session_id", out string cookieSessionId);

            // Check if cookie values exist
            if (string.IsNullOrEmpty(cookieUsername) || 
                string.IsNullOrEmpty(cookieToken) || 
                string.IsNullOrEmpty(cookieSessionId))
                return false;

            // Compare session values with cookie values
            return sessionUsername == cookieUsername && 
                  sessionToken == cookieToken && 
                  sessionId == cookieSessionId;
        }
    }
} 