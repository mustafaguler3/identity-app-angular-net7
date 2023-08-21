using System;
using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
	public class PasswordResetDto
	{
        [Required]
        public string Token { get; set; }
        [Required]
        [RegularExpression("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$", ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}

