using System;
using System.Security.Claims;
using Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
	public class VtContextSeed
	{
		private readonly VtContext _context;
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

        public VtContextSeed(VtContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task InitializeContextAsync()
        {
            if (_context.Database.GetPendingMigrationsAsync().GetAwaiter().GetResult().Count() > 0)
            {
                // applies any pending migration into our database
                await _context.Database.MigrateAsync();
            }

            if (!_roleManager.Roles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = SD.AdminRole});
                await _roleManager.CreateAsync(new IdentityRole { Name = SD.ManagerRole });
                await _roleManager.CreateAsync(new IdentityRole { Name = SD.PlayerRole });
            }

            if (!_userManager.Users.AnyAsync().GetAwaiter().GetResult())
            {
                // Admin
                var admin = new User
                {
                    FirstName = "admin",
                    LastName = "jack",
                    UserName = SD.AdminRole,
                    Email = SD.AdminRole,
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(admin, "123456");
                await _userManager.AddToRolesAsync(admin, new[] { SD.AdminRole, SD.ManagerRole, SD.PlayerRole });
                await _userManager.AddClaimsAsync(admin, new Claim[]
                {
                    new Claim(ClaimTypes.Email,admin.Email),
                    new Claim(ClaimTypes.Surname,admin.LastName)
                });

                // - ---- Manager Role -- -- -----

                var manager= new User
                {
                    FirstName = "manager",
                    LastName = "jack",
                    UserName = "manager@example.com",
                    Email = "manager@example.com",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(manager, "123456");
                await _userManager.AddToRolesAsync(manager, new[] { SD.ManagerRole });
                await _userManager.AddClaimsAsync(manager, new Claim[]
                {
                    new Claim(ClaimTypes.Email,manager.Email),
                    new Claim(ClaimTypes.Surname,manager.LastName)
                });

                // - ---- Player Role -- -- -----

                var player = new User
                {
                    FirstName = "player",
                    LastName = "jack",
                    UserName = "player@example.com",
                    Email = "player@example.com",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(player, "123456");
                await _userManager.AddToRolesAsync(player, new[] { SD.PlayerRole });
                await _userManager.AddClaimsAsync(player, new Claim[]
                {
                    new Claim(ClaimTypes.Email,player.Email),
                    new Claim(ClaimTypes.Surname,player.LastName)
                });

                // - ---- VipPlayer Role -- -- -----

                var vipPlayer = new User
                {
                    FirstName = "vipplayer",
                    LastName = "jack",
                    UserName = "vipplayer@example.com",
                    Email = "vipplayer@example.com",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(vipPlayer, "123456");
                await _userManager.AddToRolesAsync(vipPlayer, new[] { SD.PlayerRole });
                await _userManager.AddClaimsAsync(player, new Claim[]
                {
                    new Claim(ClaimTypes.Email,vipPlayer.Email),
                    new Claim(ClaimTypes.Surname,vipPlayer.LastName)
                });
            }
        }
    }
}

