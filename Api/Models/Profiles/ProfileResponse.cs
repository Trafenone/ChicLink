using Data.Models;

namespace Api.Models.Profiles;

public class ProfileResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Description { get; set; }
    public string Interests { get; set; }
    public ICollection<PhotoResponse> Photos { get; set; }

    public static ProfileResponse CreateProfileResponse(Profile profile)
    {
        var photos = profile.Photos.Select(p => new PhotoResponse()
        {
            Id = p.Id,
            Url = p.Url
        }).ToList();

        var profileResponse = new ProfileResponse()
        {
            Id = profile.Id,
            UserId = profile.UserId,
            Description = profile.Description ?? "",
            Interests = profile.Interests ?? "",
            Photos = photos
        };

        return profileResponse;
    }
}
