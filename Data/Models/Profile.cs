namespace Data.Models;

public class Profile
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public string? Interests { get; set; }

    //TODO  Add other fields

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Photo> Photos { get; set; }
}
