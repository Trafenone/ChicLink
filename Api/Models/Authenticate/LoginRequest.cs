using System.ComponentModel.DataAnnotations;

namespace Api.Models.Authenticate;

public class LoginRequest
{
    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}