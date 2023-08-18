using System;
using Api.Data;
using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly JWTService _jwtService;
		private readonly SignInManager<User> _signinManager;
		private readonly UserManager<User> _userManager;

        public AccountController(JWTService jwtService,
            SignInManager<User> signinManager,
            UserManager<User> userManager)
        {
            _jwtService = jwtService;
            _signinManager = signinManager;
            _userManager = userManager;
        }

        public async Task<ActionResult<UserDto>> Login(LoginDto login)
        {
            var user = await _userManager.FindByNameAsync(login.Username);
            if (user == null) return Unauthorized("Invalid username or password");

            if (user.EmailConfirmed == false) return Unauthorized("Please confirm you email");

            var result = await _signinManager.CheckPasswordSignInAsync(user, login.Password, false);
            if (!result.Succeeded) return Unauthorized("Invalid username or password");

            return createAppUserDto(user);
        }

        #region private helper method
        private UserDto createAppUserDto(User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Jwt = _jwtService.createJWT(user)
            };
        }
        #endregion
    }
}