using Microsoft.AspNetCore.Identity;

namespace Data.Models;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly Birthday { get; set; }
    public bool Sex { get; set; }
    public string Location { get; set; } = null!;

    public ICollection<Photo> Photos { get; set; }
    public ICollection<Like> LikesSent { get; set; }
    public ICollection<Like> LikesReceived { get; set; }
    public ICollection<Message> MessagesSent { get; set; }
    public ICollection<Message> MessagesReceived { get; set; }
}
