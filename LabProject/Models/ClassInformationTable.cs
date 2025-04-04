using System.ComponentModel.DataAnnotations;

namespace LabProject.Models
{
    public class ClassInformationTable
    {
        public int Id { get; set; }

        [Display(Name = "Class Name")]
        public string ClassName { get; set; } = string.Empty;

        [Display(Name = "Student Count")]
        public int StudentCount { get; set; }

        public string Description { get; set; } = string.Empty;
    }
} 