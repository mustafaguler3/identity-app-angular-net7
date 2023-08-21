using System;
using System.Security.Claims;
using System.Text;
using Api.Data;
using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
        private readonly IConfiguration _config;
		private readonly JWTService _jwtService;
		private readonly SignInManager<User> _signinManager;
		private readonly UserManager<User> _userManager;
        private readonly EmailService _emailService;

        public AccountController(JWTService jwtService,
            SignInManager<User> signinManager,
            UserManager<User> userManager,
            EmailService emailService)
        {
            _jwtService = jwtService;
            _signinManager = signinManager;
            _userManager = userManager;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto login)
        {
            var user = await _userManager.FindByNameAsync(login.Username);
            if (user == null) return Unauthorized("Invalid username or password");

            if (user.EmailConfirmed == false) return Unauthorized("Please confirm your email");

            var result = await _signinManager.CheckPasswordSignInAsync(user, login.Password, false);
            if (!result.Succeeded) return Unauthorized("Invalid username or password");

            return createAppUserDto(user);
        }

        [HttpPost("register")]
        public async Task<ActionResult> register(RegisterDto register)
        {
            if(await checkEmailExistsAsync(register.Email))
            {
                return BadRequest($"An existing account is using {register.Email}, email address. Please try with another email address");
            }

            var user = new User
            {
                FirstName = register.FirstName.ToLower(),
                LastName = register.LastName.ToLower(),
                UserName = register.Email.ToLower(),
                Email = register.Email.ToLower(),
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, register.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            try
            {
                if (await )
                {

                }
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to send email,Please contact admin");
            }

            return Ok("Your account has been created, you can login");
        }

        [Authorize]
        [HttpGet("refresh-user-token")]
        public async Task<ActionResult<UserDto>> refreshUserToken()
        {
            var user = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value);

            return createAppUserDto(user);
        }

        #region private helper method
        private async Task<bool> SendConfirmMailAsync(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{_config["JWT:ClientUrl"]}/{_config["Email:ConfirmationPath"]}?token={token}&email={user.Email}";

            var body = $"<p>Hello : {user.FirstName} {user.LastName}</p>" +
                "<p>Please confirm your email address by clicking on the following link</p>" +
                    $"<p><a href=\"{url}\">Click here</a></p>" +
                    "<p>Thank you</p>" +
                    $"<br>{_config["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto(user.Email, "Confirm your email", body);

            return await _emailService.SendEmailAsync(emailSend);
        }

        private UserDto createAppUserDto(User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Jwt = _jwtService.createJWT(user)
            };
        }

        private async Task<bool> checkEmailExistsAsync(string email)
        {
            return await _userManager.Users.AnyAsync(i => i.Email == email.ToLower());
        }
        #endregion
    }
}