using System;
namespace Api.DTOs
{
	public class MemberDto
	{
		public string Id { get; set; }
		public string UserName { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public bool IsLocked { get; set; }
		public DateTime DateCreated { get; set; }
		public IEnumerable<string> Roles { get; set; }
	}
}

