using Api.Models.Authenticate;
using Api.Models.Identity;
using Data;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Api.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AuthenticateController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly JwtSettings _jwtSettings;

    public AuthenticateController(UserManager<User> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _jwtSettings = new JwtSettings
        {
            ValidAudience = configuration["Audience"],
            ValidIssuer = configuration["Issuer"],
            SecretKey = configuration["SecretKey"]
        };
    }

    /// <summary>
    /// Log in a user.
    /// </summary>
    /// <remarks>
    /// Logs in a user with provided credentials and returns JWT tokens.
    /// </remarks>
    /// <param name="request">Login request.</param>
    /// <returns>Access token and refresh token.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized();

        var accessToken = GetToken([new(ClaimTypes.Email, user.Email!)]);
        var refreshToken = GenerateRefreshToken();

        return Ok(new LoginResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
            RefreshToken = refreshToken,
            Expiration = accessToken.ValidTo
        });
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    /// <remarks>
    /// Registers a new user with provided details.
    /// </remarks>
    /// <param name="request">Registration request.</param>
    /// <returns>Response status.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var userExists = _userManager.FindByEmailAsync(request.Email);
        if (userExists.Result != null)
            return BadRequest("User with this email already exists");

        var user = new User()
        {
            UserName = request.Email,
            //UserName = request.FirstName.ToLower()[0] + request.LastName.ToLower(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.Phone,
            Birthday = request.Birthday,
            Gender = request.Gender,
            Location = request.Location,
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var identityError = result.Errors.FirstOrDefault();
            return Problem(identityError?.Description);
        }

        return Created();
    }

    /// <summary>
    /// Change user password.
    /// </summary>
    /// <remarks>
    /// Changes the password for a user if the old password matches.
    /// </remarks>
    /// <param name="request">Change password request.</param>
    /// <returns>Response status.</returns>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
        if (user == null)
            return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
        if (!result.Succeeded)
            return Problem(result.Errors.FirstOrDefault()?.Description);

        return NoContent();
    }

    /// <summary>
    /// Refresh access token using refresh token.
    /// </summary>
    /// <remarks>
    /// Refreshes the JWT token if the refresh token is valid.
    /// </remarks>
    /// <param name="request">Token request.</param>
    /// <returns>New access token and refresh token.</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    public IActionResult RefreshToken([FromBody] TokenRequest request)
    {
        var principal = GetPrincipalFromExpiredToken(request.Token);
        if (principal == null)
            return BadRequest("Invalid token");

        var newJwtToken = GetToken(principal.Claims.ToList());
        var newRefreshToken = GenerateRefreshToken();

        return Ok(new TokenResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(newJwtToken),
            RefreshToken = newRefreshToken
        });
    }

    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.ValidIssuer,
            audience: _jwtSettings.ValidAudience,
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}
