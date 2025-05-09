using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LabProject.Data;
using LabProject.Models;
using LabProject.Helpers;

namespace LabProject.Pages.Classes
{
    public class IndexModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public IndexModel(SchoolDbContext context)
        {
            _context = context;
        }

        public IList<Class> ClassList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check authentication
            if (!AuthHelper.IsAuthenticated(HttpContext))
            {
                return RedirectToPage("/Login");
            }

            ClassList = await _context.Classes.ToListAsync();
            return Page();
        }
    }
} 