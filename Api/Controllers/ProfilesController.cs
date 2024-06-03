using Api.Models.Profiles;
using Data;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

/// <summary>
/// Controller for managing user profiles.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProfilesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;

    public ProfilesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    /// <summary>
    /// Get profile by profile ID.
    /// </summary>
    /// <remarks>
    /// Returns the profile details for the specified profile ID.
    /// </remarks>
    /// <param name="profileId">Profile ID.</param>
    /// <returns>Profile details.</returns>
    [HttpGet("by-profile-id/{profileId:guid}")]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Get profile by user ID.
    /// </summary>
    /// <remarks>
    /// Returns the profile details for the specified user ID.
    /// </remarks>
    /// <param name="userId">User ID.</param>
    /// <returns>Profile details.</returns>
    [HttpGet("by-user-id/{userId:guid}")]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Create a profile for a user.
    /// </summary>
    /// <remarks>
    /// Creates a new profile for the specified user ID.
    /// </remarks>
    /// <param name="userId">User ID.</param>
    /// <param name="request">Profile creation request.</param>
    /// <returns>Created profile details.</returns>
    [HttpPost("create-profile-for-user/{userId:guid}")]
    [ProducesResponseType(typeof(Created), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BadRequest), StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Update profile details.
    /// </summary>
    /// <remarks>
    /// Updates the profile details for the specified profile ID.
    /// </remarks>
    /// <param name="profileId">Profile ID.</param>
    /// <param name="request">Profile update request.</param>
    /// <returns>No content.</returns>
    [HttpPut("update-profile/{profileId:guid}")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Update profile photos.
    /// </summary>
    /// <remarks>
    /// Updates the profile photos for the specified profile ID.
    /// </remarks>
    /// <param name="profileId">Profile ID.</param>
    /// <param name="photos">List of photos.</param>
    /// <returns>No content.</returns>
    [HttpPut("update-profile-photos/{profileId:guid}")]
    [ProducesResponseType(typeof(NoContent), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
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
