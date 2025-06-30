using System.ComponentModel.DataAnnotations;

namespace Velusia.Server.ViewModels.Fido;

public class StartLogin
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    public string ReturnUrl { get; set; }
}