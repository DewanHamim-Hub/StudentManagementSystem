using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentManagementSystem.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(100)")]
        public string CourseName { get; set; } = null!;

        public int? TeacherId { get; set; }

        [ForeignKey(nameof(TeacherId))]
        [ValidateNever]
        public Teacher? Teacher { get; set; } 
    }
}