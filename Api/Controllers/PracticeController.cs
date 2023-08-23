using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PracticeController : ControllerBase
	{
		[HttpGet("admin-policy")]
		[Authorize(policy:"AdminPolicy")]
		public IActionResult AdminPolicy()
		{
			return Ok("public");
		}
	}
}

