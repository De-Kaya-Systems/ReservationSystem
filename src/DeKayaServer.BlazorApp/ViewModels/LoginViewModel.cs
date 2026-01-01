using System.ComponentModel.DataAnnotations;

namespace DeKayaServer.BlazorApp.ViewModels;

public sealed class LoginViewModel
{
    [Required]
    public string EmailOrUserName { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;

}
