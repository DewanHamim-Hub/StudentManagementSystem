using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentManagementSystem.Models
{
    public class Teacher
    {
        [Key]
        public int TeacherId { get; set; }

        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        [ValidateNever]
        public User User { get; set; } = null!;

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string TeacherName { get; set; } = null!;

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string Email { get; set; } = null!;

        [Column(TypeName = "nvarchar(50)")]
        public string? Department { get; set; } = "";
    }
}