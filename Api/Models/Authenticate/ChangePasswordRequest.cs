using System.ComponentModel.DataAnnotations;

namespace Api.Models.Authenticate
{
    public class ChangePasswordRequest
    {
        public Guid UserId { get; set; }

        [Required]
        public string OldPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
