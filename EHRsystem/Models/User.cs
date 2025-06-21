using System.ComponentModel.DataAnnotations;

namespace EHRsystem.Models
{
    public abstract class User // Marked as abstract
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } // No CS8618 warning due to [Required]

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } // No CS8618 warning due to [Required]

        [Required(ErrorMessage = "Password is required.")]
        public string PasswordHash { get; set; } // No CS8618 warning due to [Required]

        [Required]
        public string Role { get; set; } // No CS8618 warning due to [Required]
    }
}