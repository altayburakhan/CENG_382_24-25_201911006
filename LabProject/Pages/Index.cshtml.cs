using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LabProject.Models;

namespace LabProject.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public List<ClassInformationModel> Classes { get; set; }
        public ClassInformationModel NewClass { get; set; } = new();
        public ClassInformationModel? EditingClass { get; set; }
        private static int _nextId = 1;
        private static List<ClassInformationModel> _classes = new();
        private static bool _isInitialized = false;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            Classes = _classes;
            
            // Sample data for initial load - only once
            if (!_isInitialized)
            {
                _classes.Add(new ClassInformationModel
                {
                    Id = _nextId++,
                    ClassName = "CENG 101",
                    StudentCount = 30,
                    Description = "Introduction to Programming"
                });

                _classes.Add(new ClassInformationModel
                {
                    Id = _nextId++,
                    ClassName = "CENG 102",
                    StudentCount = 25,
                    Description = "Object-Oriented Programming"
                });

                _isInitialized = true;
            }
        }

        public void OnGet(int? editId = null)
        {
            if (editId.HasValue)
            {
                EditingClass = Classes.FirstOrDefault(c => c.Id == editId.Value);
            }
            else
            {
                // Reset NewClass when not editing
                NewClass = new ClassInformationModel();
            }
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

            var existingClass = Classes.FirstOrDefault(c => c.Id == editId);
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
            var classToDelete = Classes.FirstOrDefault(c => c.Id == id);
            if (classToDelete != null)
            {
                Classes.Remove(classToDelete);
            }
            return RedirectToPage();
        }
    }
}
