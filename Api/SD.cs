using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Api
{
	public static class SD
	{
		public const string AdminRole = "Admin";
		public const string ManagerRole = "Manager";
		public const string PlayerRole = "Player";

		public const string AdminUserName = "mustafa@example.com";
		public const string SuperAdminChangeNotAllowed = "Super Admin change is not allowed";

		public const int MaximumLoginAttempt = 3;

		public static bool VIPPolicy(AuthorizationHandlerContext context)
		{
			if(context.User.IsInRole(PlayerRole) && context.User.HasClaim(c => c.Type == ClaimTypes.Email && c.Value.Contains("vip")))
			{
				return true;
			}
			return false;
		}
	}
}

