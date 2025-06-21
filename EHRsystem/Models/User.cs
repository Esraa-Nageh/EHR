using System.ComponentModel.DataAnnotations;

namespace EHRsystem.Models
{
    public abstract class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public required string Name { get; set; } // <<-- CHANGED THIS LINE

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public required string Email { get; set; } // <<-- CHANGED THIS LINE

        [Required(ErrorMessage = "Password is required.")]
        public required string PasswordHash { get; set; } // <<-- CHANGED THIS LINE

        [Required]
        public required string Role { get; set; } // <<-- CHANGED THIS LINE
    }
}