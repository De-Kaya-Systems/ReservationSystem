using System.ComponentModel.DataAnnotations;

namespace DeKayaServer.BlazorApp.ViewModels;

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "Yeni şifre gereklidir")]
    [MinLength(8, ErrorMessage = "Şifre en az 8 karakter olmalıdır")]
    public string NewPassword { get; set; } = null!;

    [Required(ErrorMessage = "Şifre tekrarı gereklidir")]
    [Compare(nameof(NewPassword), ErrorMessage = "Şifreler eşleşmiyor, yiğidim!")]
    public string ConfirmPassword { get; set; } = null!;

}
