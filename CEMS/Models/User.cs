using System.ComponentModel.DataAnnotations;

namespace CEMS.Models
{

    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Email { get; set; }
        
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
        
        public UserStatus Status { get; set; } = UserStatus.Active;

        public bool IsDeleted { get; set; } = false;
        
        public string? ResetToken { get; set; }
        
        public DateTime? ResetTokenExpiry { get; set; }

    }
}