using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LabProject.Models;

namespace LabProject.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public List<ClassInformationModel> Classes { get; set; } = new();
        public ClassInformationModel NewClass { get; set; } = new();
        public ClassInformationModel? EditingClass { get; set; }
        private static int _nextId = 1;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(int? editId = null)
        {
            // Sample data for initial load
            if (!Classes.Any())
            {
                Classes.Add(new ClassInformationModel
                {
                    Id = _nextId++,
                    ClassName = "CENG 101",
                    StudentCount = 30,
                    Description = "Introduction to Programming"
                });

                Classes.Add(new ClassInformationModel
                {
                    Id = _nextId++,
                    ClassName = "CENG 102",
                    StudentCount = 25,
                    Description = "Object-Oriented Programming"
                });
            }

            if (editId.HasValue)
            {
                EditingClass = Classes.FirstOrDefault(c => c.Id == editId.Value);
            }
        }

        public IActionResult OnPostAdd()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            NewClass.Id = _nextId++;
            Classes.Add(NewClass);
            return RedirectToPage();
        }

        public IActionResult OnPostEdit()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingClass = Classes.FirstOrDefault(c => c.Id == EditingClass.Id);
            if (existingClass != null)
            {
                existingClass.ClassName = EditingClass.ClassName;
                existingClass.StudentCount = EditingClass.StudentCount;
                existingClass.Description = EditingClass.Description;
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
