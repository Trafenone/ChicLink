namespace Api.Models.Profiles;

public class CreateProfileRequest
{
    public Guid UserId { get; set; }
    public string Description { get; set; }
    public string? Interests { get; set; }
    public List<IFormFile>? Photos { get; set; }
}
