using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
	public class VtContext : IdentityDbContext<User>
	{
		public VtContext(DbContextOptions<VtContext> options):base(options)
		{
		}

		public DbSet<User> Users { get; set; }
	}
}

