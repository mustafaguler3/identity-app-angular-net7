using System;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PlayController : ControllerBase
	{
		[HttpGet("get-players")]
		public IActionResult Players()
		{
			return Ok(new JsonResult(new { message = "only authorized users an view players" }));
		}
	}
}

