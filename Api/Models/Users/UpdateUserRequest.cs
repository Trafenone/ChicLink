namespace Api.Models.Users;

public class UpdateUserRequest
{
    public Guid UserId { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Location { get; set; }
}
