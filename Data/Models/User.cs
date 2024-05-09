using Microsoft.AspNetCore.Identity;

namespace Data.Models;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly BirthDay { get; set; }
    public bool Sex { get; set; }
    public string Location { get; set; } = null!;
}
