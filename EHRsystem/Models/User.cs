using System.ComponentModel.DataAnnotations;

namespace EHRsystem.Models
{
    public abstract class User  // 🔧 Marked as abstract
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; } // "Doctor", "Patient", or "Admin"
    }
}
