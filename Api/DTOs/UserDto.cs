using System;
using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
	public class UserDto
	{
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Jwt { get; set; }
    }
}

