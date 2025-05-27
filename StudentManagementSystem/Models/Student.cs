using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentManagementSystem.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        [ValidateNever]
        public User User { get; set; } = null!;

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string StudentName { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string Email { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string Department { get; set; }
    }
}