using System.ComponentModel.DataAnnotations;

namespace DeKayaServer.BlazorApp.ViewModels;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "E-posta veya kullanıcı adı gereklidir")]
    [MinLength(3, ErrorMessage = "En az 3 karakter olmalıdır")]
    public string EmailOrUserName { get; set; } = null!;

    [Required(ErrorMessage = "Parola gir!")]
    [MinLength(6, ErrorMessage = "En az 6 karakter olmalıdır")]
    public string Password { get; set; } = null!;

}
