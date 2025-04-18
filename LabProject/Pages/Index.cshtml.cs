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

        // Sorting properties
        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "ClassName";
        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; } = "asc";

        // Export property
        [BindProperty(SupportsGet = true)]
        public bool ExportAll { get; set; }
        [BindProperty(SupportsGet = true)]
        public List<string> SelectedProperties { get; set; } = new();

        [BindProperty]
        public ClassInformationTable EditClass { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SelectedIds { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ExportColumn { get; set; }

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

            // Apply sorting
            var sortByLower = SortBy?.ToLower() ?? "classname";
            var sortOrderLower = SortOrder.ToLower();

            if (sortByLower == "classname")
            {
                filteredClasses = sortOrderLower == "asc"
                    ? filteredClasses.OrderBy(c => c.ClassName)
                    : filteredClasses.OrderByDescending(c => c.ClassName);
            }
            else if (sortByLower == "studentcount")
            {
                filteredClasses = sortOrderLower == "asc"
                    ? filteredClasses.OrderBy(c => c.StudentCount)
                    : filteredClasses.OrderByDescending(c => c.StudentCount);
            }
            else if (sortByLower == "description")
            {
                filteredClasses = sortOrderLower == "asc"
                    ? filteredClasses.OrderBy(c => c.Description)
                    : filteredClasses.OrderByDescending(c => c.Description);
            }
            else
            {
                filteredClasses = filteredClasses.OrderBy(c => c.ClassName);
            }

            // Handle JSON export
            if (!string.IsNullOrEmpty(ExportColumn))
            {
                object columnData = ExportColumn.ToLower() switch
                {
                    "classname" => filteredClasses.Select(c => new { ClassName = c.ClassName }).ToList(),
                    "studentcount" => filteredClasses.Select(c => new { StudentCount = c.StudentCount }).ToList(),
                    "description" => filteredClasses.Select(c => new { Description = c.Description }).ToList(),
                    _ => filteredClasses.Select(c => new { c.ClassName, c.StudentCount, c.Description }).ToList()
                };

                var json = JsonSerializer.Serialize(columnData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                return new FileContentResult(System.Text.Encoding.UTF8.GetBytes(json), "application/json")
                {
                    FileDownloadName = $"{ExportColumn.ToLower()}_export.json"
                };
            }
            else if (ExportAll)
            {
                var json = JsonSerializer.Serialize(filteredClasses.ToList(), new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                return new FileContentResult(System.Text.Encoding.UTF8.GetBytes(json), "application/json")
                {
                    FileDownloadName = "all_classes_export.json"
                };
            }
            else if (!string.IsNullOrEmpty(SelectedIds))
            {
                var selectedIdList = SelectedIds.Split(',').Select(int.Parse).ToList();
                var selectedClasses = filteredClasses.Where(c => selectedIdList.Contains(c.Id)).ToList();

                var json = JsonSerializer.Serialize(selectedClasses, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                return new FileContentResult(System.Text.Encoding.UTF8.GetBytes(json), "application/json")
                {
                    FileDownloadName = "selected_classes_export.json"
                };
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

            return Page();
        }

        public IActionResult OnPostAdd()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _dataService.AddClass(NewClass);
            
            // Preserve all query parameters
            return RedirectToPage(new
            {
                currentPage = CurrentPage,
                classNameFilter = ClassNameFilter,
                minStudentCount = MinStudentCount,
                maxStudentCount = MaxStudentCount,
                sortBy = SortBy,
                sortOrder = SortOrder
            });
        }

        public IActionResult OnPostEdit()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _dataService.UpdateClass(EditClass);

            // Preserve all query parameters
            return RedirectToPage(new
            {
                currentPage = CurrentPage,
                classNameFilter = ClassNameFilter,
                minStudentCount = MinStudentCount,
                maxStudentCount = MaxStudentCount,
                sortBy = SortBy,
                sortOrder = SortOrder
            });
        }

        public IActionResult OnPostDelete(int id)
        {
            _dataService.DeleteClass(id);
            
            // Preserve all query parameters for delete operation too
            return RedirectToPage(new
            {
                currentPage = CurrentPage,
                classNameFilter = ClassNameFilter,
                minStudentCount = MinStudentCount,
                maxStudentCount = MaxStudentCount,
                sortBy = SortBy,
                sortOrder = SortOrder
            });
        }
    }
}
