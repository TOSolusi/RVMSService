using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RVMSService.Models
{
    // Add this new class
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public DateTime? LastLoginTime { get; set; }
    }

    public class UserRegisterModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Email { get; set; } // Optional
        public string? FullName { get; set; }
    }

    public class UserLoginModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class AssignRoleModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Role { get; set; }
    }

    public class UserModel
    {
        public string UserName { get; set; }

        public string Email { get; set; } // Optional
        public string role { get; set; }
        public string? FullName { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public bool? IsLockedOut { get; set; } 
    }
}
