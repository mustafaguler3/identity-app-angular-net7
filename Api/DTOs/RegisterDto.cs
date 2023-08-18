using System;
using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
	public class RegisterDto
	{
		[Required]
        [StringLength(maximumLength: 15,MinimumLength = 3,ErrorMessage = "First name must be at least {2},and maximum {1} characters")]
		public string FirstName { get; set; }
        [Required]
        [StringLength(maximumLength: 15, MinimumLength = 3, ErrorMessage = "Last name must be at least {2},and maximum {1} characters")]
        public string LastName { get; set; }
        [Required]
        [RegularExpression("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$",ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        [Required]
        [StringLength(maximumLength: 15, MinimumLength = 6, ErrorMessage = "Password must be at least {2},and maximum {1} characters")]
        public string Password { get; set; }
	}
}

