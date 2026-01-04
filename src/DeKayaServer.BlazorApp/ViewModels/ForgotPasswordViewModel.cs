using System.ComponentModel.DataAnnotations;

namespace DeKayaServer.BlazorApp.ViewModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "E-posta adresi giriniz")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    public string Email { get; set; } = null!;
}
