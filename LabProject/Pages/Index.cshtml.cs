using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LabProject.Models;
using LabProject.Data;
using Microsoft.EntityFrameworkCore;
using LabProject.Helpers;
using System.Linq;
using LabProject.Helpers;
using System.Text.Json;
using LabProject.Services;

namespace LabProject.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SchoolDbContext _context;
        private readonly ILogger<IndexModel> _logger;
        private readonly DataService _dataService;
        public List<Class> Classes { get; set; } = new();
        [BindProperty]
        public Class NewClass { get; set; } = new();
        public Class EditClass { get; set; } = new();

        // Pagination properties
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

        // Filter properties
        [BindProperty(SupportsGet = true)]
        public string? ClassNameFilter { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? MinStudentCount { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? MaxStudentCount { get; set; }

        // Sorting properties
        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; } = "ClassName";
        [BindProperty(SupportsGet = true)]
        public string? SortOrder { get; set; } = "asc";

        // Export property
        [BindProperty(SupportsGet = true)]
        public bool ExportAll { get; set; }
        [BindProperty(SupportsGet = true)]
        public List<string> SelectedProperties { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SelectedIds { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ExportColumn { get; set; }

        public IndexModel(SchoolDbContext context, ILogger<IndexModel> logger, DataService dataService)
        {
            _context = context;
            _logger = logger;
            _dataService = dataService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!AuthHelper.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            // Eğer veritabanında hiç kayıt yoksa örnek veri ekle
            if (!await _context.Classes.AnyAsync())
            {
                var random = new Random();
                var sampleClasses = new List<Class>();
                for (int i = 1; i <= 100; i++)
                {
                    sampleClasses.Add(new Class
                    {
                        Name = $"Class {i}",
                        PersonCount = random.Next(1, 100),
                        Description = $"Description for Class {i}",
                        IsActive = true
                    });
                }
                _context.Classes.AddRange(sampleClasses);
                await _context.SaveChangesAsync();
            }

            Classes = await _context.Classes.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            Console.WriteLine($"Name: {NewClass.Name}, PersonCount: {NewClass.PersonCount}, Description: {NewClass.Description}, IsActive: {NewClass.IsActive}");
            if (!AuthHelper.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["AddError"] = string.Join(" | ", errors);
                return await OnGetAsync();
            }

            _context.Classes.Add(NewClass);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!AuthHelper.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            if (!ModelState.IsValid)
                return await OnGetAsync();

            var classToUpdate = await _context.Classes.FindAsync(EditClass.Id);
            if (classToUpdate == null)
                return NotFound();

            classToUpdate.Name = EditClass.Name;
            classToUpdate.PersonCount = EditClass.PersonCount;
            classToUpdate.Description = EditClass.Description;
            classToUpdate.IsActive = EditClass.IsActive;
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!AuthHelper.IsAuthenticated(HttpContext))
                return RedirectToPage("/Login");

            var classToDelete = await _context.Classes.FindAsync(id);
            if (classToDelete == null)
                return NotFound();

            _context.Classes.Remove(classToDelete);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
