using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace VideoGameHash.Models
{
    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        public LoginModel()
        {

        }
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Security Question")]
        public string SecurityQuestion { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Security Answer")]
        public string SecurityAnswer { get; set; }
    }

    public class RecoverModel
    {
        [Required]
        [Display(Name = "Email Address")]
        public string Email { get; set; }
    }

    public class SecurityQuestionModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Security Question")]
        public string SecurityQuestion { get; set; }

        [Required]
        [Display(Name = "Security Answer")]
        public string SecurityAnswer { get; set; }
    }

    public class SecurityAnswerModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Security Question")]
        public string SecurityQuestion { get; set; }

        [Required]
        [Display(Name = "Security Answer")]
        public string SecurityAnswer { get; set; }
    }

    public class UserMasterModel
    {
        public UserProfile UserProfile { get; set; }
        public Membership UserMembership { get; set; }
        public List<string> UsersRoles;

        public UserMasterModel()
        {
            UsersRoles = new List<string>();
        }
    }

    public class RolesModel
    {
        [Required]
        public string RoleName { get; set; }
    }

    public class UserRole
    {
        public UserProfile UserProfile { get; set; }

        public Models.Roles Roles { get; set; }
    }
}