using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LabProject.Models;
using System.Linq;

namespace LabProject.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public List<ClassInformationTable> Classes { get; set; }
        public ClassInformationModel NewClass { get; set; } = new();
        public ClassInformationModel? EditingClass { get; set; }
        private static int _nextId = 1;
        private static List<ClassInformationModel> _classes = new();
        private static bool _isInitialized = false;

        // Pagination properties
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        // Filter properties
        [BindProperty(SupportsGet = true)]
        public string? ClassNameFilter { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? MinStudentCount { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? MaxStudentCount { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            Classes = new List<ClassInformationTable>();
            
            // Sample data for initial load - only once
            if (!_isInitialized)
            {
                var random = new Random();
                for (int i = 0; i < 100; i++)
                {
                    _classes.Add(new ClassInformationModel
                    {
                        Id = _nextId++,
                        ClassName = $"CENG {random.Next(100, 500)}",
                        StudentCount = random.Next(20, 50),
                        Description = $"Description for class {i + 1}"
                    });
                }

                _isInitialized = true;
            }
        }

        public void OnGet(int? editId = null)
        {
            if (editId.HasValue)
            {
                EditingClass = _classes.FirstOrDefault(c => c.Id == editId.Value);
            }
            else
            {
                // Reset NewClass when not editing
                NewClass = new ClassInformationModel();
            }

            // Apply filters
            var filteredClasses = _classes.AsQueryable();

            if (!string.IsNullOrEmpty(ClassNameFilter))
            {
                filteredClasses = filteredClasses.Where(c => 
                    c.ClassName.Contains(ClassNameFilter, StringComparison.OrdinalIgnoreCase));
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
            var paginatedClasses = filteredClasses
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .Select(c => new ClassInformationTable
                {
                    Id = c.Id,
                    ClassName = c.ClassName,
                    StudentCount = c.StudentCount,
                    Description = c.Description
                })
                .ToList();

            Classes = paginatedClasses;
        }

        public IActionResult OnPostAdd()
        {
            // Log all form values for debugging
            _logger.LogInformation("Form values received:");
            _logger.LogInformation($"ClassName: {Request.Form["NewClass.ClassName"]}");
            _logger.LogInformation($"StudentCount: {Request.Form["NewClass.StudentCount"]}");
            _logger.LogInformation($"Description: {Request.Form["NewClass.Description"]}");

            // Manually bind form values
            NewClass.ClassName = Request.Form["NewClass.ClassName"].ToString();
            if (int.TryParse(Request.Form["NewClass.StudentCount"].ToString(), out int studentCount))
            {
                NewClass.StudentCount = studentCount;
            }
            NewClass.Description = Request.Form["NewClass.Description"].ToString();

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state: {errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return Page();
            }

            // Log the values for debugging
            _logger.LogInformation($"Adding new class: Name={NewClass.ClassName}, Count={NewClass.StudentCount}, Description={NewClass.Description}");

            var newClass = new ClassInformationModel
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
            // Log form values for debugging
            _logger.LogInformation("Edit form values received:");
            _logger.LogInformation($"ClassName: {Request.Form["EditingClass.ClassName"]}");
            _logger.LogInformation($"StudentCount: {Request.Form["EditingClass.StudentCount"]}");
            _logger.LogInformation($"Description: {Request.Form["EditingClass.Description"]}");
            _logger.LogInformation($"Id: {Request.Form["EditingClass.Id"]}");

            if (!int.TryParse(Request.Form["EditingClass.Id"].ToString(), out int editId))
            {
                _logger.LogWarning("Invalid edit ID");
                return Page();
            }

            var existingClass = _classes.FirstOrDefault(c => c.Id == editId);
            if (existingClass != null)
            {
                // Manually bind form values
                existingClass.ClassName = Request.Form["EditingClass.ClassName"].ToString();
                if (int.TryParse(Request.Form["EditingClass.StudentCount"].ToString(), out int studentCount))
                {
                    existingClass.StudentCount = studentCount;
                }
                existingClass.Description = Request.Form["EditingClass.Description"].ToString();

                _logger.LogInformation($"Updated class: Name={existingClass.ClassName}, Count={existingClass.StudentCount}, Description={existingClass.Description}");
            }
            else
            {
                _logger.LogWarning($"Class with ID {editId} not found");
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
