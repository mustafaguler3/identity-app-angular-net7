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

            if (result.IsLockedOut)
            {
                return Unauthorized(string.Format("Your account has been locked.You should wait until {0} (UTC time) to be able to login",user.LockoutEnd));
            }
            if (!result.Succeeded) return Unauthorized("Invalid username or password");

            return await createAppUserDto(user);
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
                if (await SendConfirmMailAsync(user))
                {
                    return Ok(new JsonResult(new { title = "Account created", message = "Your account has been created,please confirm your account" }));
                }
                return BadRequest("Failed to send email,Please contact admin");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to send email,Please contact admin");
            }

        }

        [HttpPut("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto confirmEmail)
        {
            var user = await _userManager.FindByEmailAsync(confirmEmail.Email);

            if (user.EmailConfirmed = true) return BadRequest("Your email was confirmed before,Please login to your account");

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(confirmEmail.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

                if (result.Succeeded) return Ok(new JsonResult(new { title = "Email confirmed", message = "Your email address is confirmed,You can login now" }));

                return BadRequest("Invalid token, Please try again");
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid token, Please try again");
            }
        }

        [HttpPut("resend-email-confirmation-link/{email}")]
        public async Task<IActionResult> ResendMailConfirmationLink(string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Invalid email");
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return Unauthorized("This email address has not been registered yet");

            if (user.EmailConfirmed = true) return BadRequest("Your email address was confirmed before");

            try
            {
                if(await SendConfirmMailAsync(user))
                {
                    return Ok(new JsonResult(new { title = "Confirmation link sent", message = "Please confirm your email address" }));
                }
                return BadRequest("Failed to send email,Please contact admin");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to send email,Please contact admin");
            }
        }

        [HttpPost("forgot-username-or-password/{email}")]
        public async Task<IActionResult> ForgotUsernameOrPassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Invalid email");
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return Unauthorized("This email address has not registered yet");

            if (user.EmailConfirmed == false) return BadRequest("Please confirm your email address");

            try
            {
                if(await SendForgotUsernameOrPasswordEmail(user))
                {
                    return Ok(new JsonResult(new { title = "Forgot username or password email sent", message = "Please check your email" }));
                }

                return BadRequest("Failed to send email,Please contact admin");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to send email,Please contact admin");
            }
        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword(PasswordResetDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null) return Unauthorized("This email address has not registered yet");

            if (user.EmailConfirmed == false) return BadRequest("Please confirm your email address");

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ResetPasswordAsync(user, decodedToken,model.NewPassword);

                if (result.Succeeded) return Ok(new JsonResult(new { title = "Password reset success", message = "Your password has been reset" }));

                return BadRequest("Invalid token, Please try again");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to send email,Please contact admin");
            }
        }

        [Authorize]
        [HttpGet("refresh-user-token")]
        public async Task<ActionResult<UserDto>> refreshUserToken()
        {
            var user = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value);

            return await createAppUserDto(user);
        }

        #region private helper method

        private async Task<bool> SendForgotUsernameOrPasswordEmail(User user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{_config["JWT:ClientUrl"]}/{_config["Email:ConfirmationPath"]}?token={token}&email={user.Email}";

            var body = $"<p>Hello : {user.FirstName} {user.LastName}</p>" +
                    $"<p>{user.UserName}</p>" +
                    "<p>in order to reset your password,please click on the following link</p>" +
                    $"<p><a href=\"{url}\">Click here</a></p>"+
                    "<p>thank you</p>" +
                    $"<br>{_config["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto(user.Email, "Forgot username or password", body);

            return await _emailService.SendEmailAsync(emailSend);
        }

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

        private async Task<UserDto> createAppUserDto(User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Jwt = await _jwtService.createJWT(user)
            };
        }

        private async Task<bool> checkEmailExistsAsync(string email)
        {
            return await _userManager.Users.AnyAsync(i => i.Email == email.ToLower());
        }
        #endregion
    }
}