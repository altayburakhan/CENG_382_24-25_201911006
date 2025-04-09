using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LabProject.Models;
using System.Linq;
using LabProject.Helpers;
using System.Text.Json;
using LabProject.Services;

namespace LabProject.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly DataService _dataService;
        public List<ClassInformationTable> Classes { get; set; } = new();
        public ClassInformationTable NewClass { get; set; } = new();
        public ClassInformationTable? EditingClass { get; set; }

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

        public IndexModel(ILogger<IndexModel> logger, DataService dataService)
        {
            _logger = logger;
            _dataService = dataService;
        }

        public IActionResult OnGet()
        {
            // Get all classes from DataService
            var allClasses = _dataService.GetClasses();

            // Filter classes based on input
            var filteredClasses = allClasses.AsQueryable();

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
            TotalItems = filteredClasses.Count();
            TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
            CurrentPage = Math.Max(1, Math.Min(CurrentPage, TotalPages));

            // Apply pagination
            Classes = filteredClasses
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Handle JSON export
            if (ExportAll)
            {
                object exportData;
                if (SelectedProperties.Any())
                {
                    var dictList = new List<Dictionary<string, object>>();
                    foreach (var item in Classes)
                    {
                        var dict = new Dictionary<string, object>();
                        foreach (var prop in SelectedProperties)
                        {
                            var value = item.GetType().GetProperty(prop)?.GetValue(item);
                            dict[prop] = value ?? string.Empty;
                        }
                        dictList.Add(dict);
                    }
                    exportData = dictList;
                }
                else
                {
                    exportData = Classes;
                }

                var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                return new FileContentResult(System.Text.Encoding.UTF8.GetBytes(json), "application/json")
                {
                    FileDownloadName = "classes_export.json"
                };
            }

            return Page();
        }

        public IActionResult OnPostAdd()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _dataService.AddClass(NewClass);
            return RedirectToPage();
        }

        public IActionResult OnPostEdit()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _dataService.UpdateClass(EditingClass!);
            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)
        {
            _dataService.DeleteClass(id);
            return RedirectToPage();
        }
    }
}
