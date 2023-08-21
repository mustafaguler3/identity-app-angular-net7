using System;
using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
	public class ConfirmEmailDto
	{
		[Required]
		public string Token { get; set; }
		[Required]
		[RegularExpression("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$",ErrorMessage = "Invalid email address")]
		public string Email { get; set; }
	}
}

