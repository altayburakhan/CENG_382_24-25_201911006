using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LabProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabProject.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        private readonly IWebHostEnvironment _environment;

        public LoginModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPostAsync()
        {
            string usersFilePath = Path.Combine(_environment.WebRootPath, "data", "users.json");
            if (!System.IO.File.Exists(usersFilePath))
            {
                ErrorMessage = "User database not found.";
                return Page();
            }

            string usersJson = System.IO.File.ReadAllText(usersFilePath);
            List<User> users = JsonSerializer.Deserialize<List<User>>(usersJson);

            // Find the user and verify credentials
            User user = users.FirstOrDefault(u => 
                u.Username == Username && 
                u.Password == Password && 
                u.IsActive);

            if (user == null)
            {
                ErrorMessage = "Username or password is incorrect.";
                return Page();
            }

            // Generate a simple token
            string token = GenerateToken(Username);
            string sessionId = HttpContext.Session.Id;

            // Store values in session
            HttpContext.Session.SetString("username", Username);
            HttpContext.Session.SetString("token", token);
            HttpContext.Session.SetString("session_id", sessionId);

            // Store values in cookies
            CookieOptions cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddMinutes(30),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            Response.Cookies.Append("username", Username, cookieOptions);
            Response.Cookies.Append("token", token, cookieOptions);
            Response.Cookies.Append("session_id", sessionId, cookieOptions);

            // Redirect to the table page
            return RedirectToPage("/Index");
        }

        private string GenerateToken(string username)
        {
            // Simple token generation - in real applications, use a more secure method
            string input = username + DateTime.UtcNow.ToString() + "secret_key";
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
} 