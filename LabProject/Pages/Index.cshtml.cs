using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LabProject.Models;
using System.Linq;
using LabProject.Helpers;
using System.Text.Json;

namespace LabProject.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public List<ClassInformationTable> Classes { get; set; } = new();
        public ClassInformationTable NewClass { get; set; } = new();
        public ClassInformationTable? EditingClass { get; set; }
        private static int _nextId = 1;
        private static List<ClassInformationTable> _classes = new();
        private static bool _isInitialized = false;

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

        // Export properties
        [BindProperty(SupportsGet = true)]
        public bool ExportAll { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<string> SelectedProperties { get; set; } = new();

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            if (!_isInitialized)
            {
                GenerateSampleData();
                _isInitialized = true;
            }
        }

        private void GenerateSampleData()
        {
            var random = new Random();
            for (int i = 1; i <= 100; i++)
            {
                _classes.Add(new ClassInformationTable
                {
                    Id = i,
                    ClassName = $"Class {i}",
                    StudentCount = random.Next(1, 100),
                    Description = $"Description for Class {i}"
                });
            }
        }

        public IActionResult OnGet()
        {
            // Filter classes based on input
            var filteredClasses = _classes.AsQueryable();

            if (!string.IsNullOrEmpty(ClassNameFilter))
            {
                filteredClasses = filteredClasses.Where(c => c.ClassName.Contains(ClassNameFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (MinStudentCount.HasValue)
            {
                filteredClasses = filteredClasses.Where(c => c.StudentCount >= MinStudentCount.Value);
            }

            if (MaxStudentCount.HasValue)
            {
                filteredClasses = filteredClasses.Where(c => c.StudentCount <= MaxStudentCount.Value);
            }

            // Calculate pagination
            TotalPages = (int)Math.Ceiling(filteredClasses.Count() / (double)PageSize);
            CurrentPage = Math.Max(1, Math.Min(CurrentPage, TotalPages));

            // Apply pagination
            Classes = filteredClasses
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Handle JSON export
            if (ExportAll)
            {
                var exportData = filteredClasses.ToList();
                var json = Utils.Instance.ExportToJson(exportData, SelectedProperties);
                
                return new FileContentResult(
                    System.Text.Encoding.UTF8.GetBytes(json),
                    "application/json")
                {
                    FileDownloadName = $"classes_export_{DateTime.Now:yyyyMMddHHmmss}.json"
                };
            }

            return Page();
        }

        public IActionResult OnPostAdd()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state: {errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return Page();
            }

            var newClass = new ClassInformationTable
            {
                Id = _nextId++,
                ClassName = NewClass.ClassName,
                StudentCount = NewClass.StudentCount,
                Description = NewClass.Description
            };

            _classes.Add(newClass);
            return RedirectToPage();
        }

        public IActionResult OnPostEdit()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingClass = _classes.FirstOrDefault(c => c.Id == EditingClass?.Id);
            if (existingClass != null)
            {
                existingClass.ClassName = EditingClass?.ClassName ?? string.Empty;
                existingClass.StudentCount = EditingClass?.StudentCount ?? 0;
                existingClass.Description = EditingClass?.Description ?? string.Empty;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)
        {
            var classToDelete = _classes.FirstOrDefault(c => c.Id == id);
            if (classToDelete != null)
            {
                _classes.Remove(classToDelete);
            }
            return RedirectToPage();
        }
    }
}
