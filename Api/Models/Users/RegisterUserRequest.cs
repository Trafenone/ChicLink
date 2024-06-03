using System.ComponentModel.DataAnnotations;

namespace Api.Models.Users;

public class RegisterUserRequest
{
    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string Phone { get; set; } = null!;

    [Required]
    public DateOnly Birthday { get; set; }

    [Required]
    public bool Sex { get; set; }

    [Required] 
    public string Location { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = null!;

    public List<IFormFile> ProfilePhotos { get; set; }
}
