using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentManagementSystem.Models
{
    public class Grade
    {
        [Key]
        public int GradeId { get; set; }

        [Required]
        public int EnrollmentId { get; set; }

        [ForeignKey(nameof(EnrollmentId))]
        [ValidateNever]
        public Enrollment Enrollment { get; set; } = null!;

        [Column(TypeName = "nvarchar(5)")]
        public string? LetterGrade { get; set; }
    }
}