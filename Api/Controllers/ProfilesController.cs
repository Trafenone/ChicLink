using Api.Models.Profiles;
using Data;
using Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProfilesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;

    public ProfilesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    [HttpGet("by-profile-id/{profileId:guid}")]
    public async Task<IActionResult> GetProfileByProfileId(Guid profileId)
    {
        var profile = await _context.Profiles
           .Include(p => p.Photos)
           .FirstOrDefaultAsync(p => p.Id == profileId);

        if (profile == null)
            return NotFound("Profile not found");

        var response = ProfileResponse.CreateProfileResponse(profile);

        return Ok(response);
    }

    [HttpGet("by-user-id/{userId:guid}")]
    public async Task<IActionResult> GetProfileByUserId(Guid userId)
    {
        var profile = await _context.Profiles
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            return NotFound("Profile not found");

        var response = ProfileResponse.CreateProfileResponse(profile);

        return Ok(response);
    }

    [HttpPost("create-profile-for-user/{userId:guid}")]
    public async Task<IActionResult> CreateProfileForUser(Guid userId, [FromForm] CreateProfileRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return NotFound("User not found");

        var existingProfile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (existingProfile != null)
            return BadRequest("Profile already exists for this user");

        var profile = new Profile
        {
            Description = request.Description,
            Interests = request.Interests,
            UserId = userId
        };

        _context.Profiles.Add(profile);
        await _context.SaveChangesAsync();

        if (request.Photos != null && request.Photos.Count > 0)
        {
            var uploadsFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolderPath))
                Directory.CreateDirectory(uploadsFolderPath);

            await AddPhoto(request.Photos, uploadsFolderPath, profile.Id);
        }

        return Created();
    }

    [HttpPut("update-profile/{profileId:guid}")]
    public async Task<IActionResult> UpdateProfile(Guid profileId, [FromBody] UpdateProfileRequest request)
    {
        var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.Id == profileId);
        if (profile == null)
            return NotFound("Profile not found");

        profile.Description = request.Description;
        profile.Interests = request.Interests;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("update-profile-photos/{profileId:guid}")]
    public async Task<IActionResult> UpdateProfilePhotos(Guid profileId, [FromForm] List<IFormFile> photos)
    {
        var profile = await _context.Profiles.Include(p => p.Photos).FirstOrDefaultAsync(p => p.Id == profileId);
        if (profile == null)
            return NotFound("Profile not found");

        var uploadsFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "uploads");

        foreach (var photo in profile.Photos)
        {
            var filePath = Path.Combine(uploadsFolderPath, Path.GetFileName(photo.Url));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.Photos.Remove(photo);
        }

        await _context.SaveChangesAsync();

        await AddPhoto(photos, uploadsFolderPath, profileId);

        return NoContent();
    }

    private async Task AddPhoto(List<IFormFile> photos, string uploadsFolderPath, Guid profileId)
    {
        foreach (var file in photos)
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var photo = new Photo
            {
                ProfileId = profileId,
                Url = $"/uploads/{fileName}"
            };

            await _context.Photos.AddAsync(photo);
            await _context.SaveChangesAsync();
        }
    }
}
