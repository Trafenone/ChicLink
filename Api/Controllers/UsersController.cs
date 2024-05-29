using Api.Models.Users;
using Data;
using Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public UsersController(ApplicationDbContext context, UserManager<User> userManager, IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        return Ok(await _context.Users.ToListAsync());
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null && !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized();

        var accessToken = GetToken([new(ClaimTypes.Email, user.Email!)]);

        var refreshToken = await _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken");
        await _userManager.SetAuthenticationTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken", refreshToken);

        return Ok(new
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
            RefreshToken = refreshToken,
            Expiration = accessToken.ValidTo
        });
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var userExists = _userManager.FindByEmailAsync(request.Email);
        if (userExists.Result != null)
            return BadRequest("User with this email already exists");

        var user = new User()
        {
            UserName = request.FirstName.ToLower()[0] + request.LastName.ToLower(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.Phone,
            Birthday = request.Birthday,
            Sex = request.Sex,
            Location = request.Location
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var identityError = result.Errors.FirstOrDefault();
            return Problem(identityError?.Description);
        }

        return Created();
    }

    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:ValidIssuer"],
            audience: _configuration["JwtSettings:ValidAudience"],
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }
}
