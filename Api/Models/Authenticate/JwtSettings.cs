namespace Api.Models.Authenticate;

public class JwtSettings
{
    public string SecretKey { get; set; }
    public string ValidIssuer { get; set; }
    public string ValidAudience { get; set; }
}
