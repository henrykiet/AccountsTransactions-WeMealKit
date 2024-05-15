using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_DataAccess.Models
{
	public class AccountsTransactionsContext : IdentityDbContext<User>
	{
		public AccountsTransactionsContext(DbContextOptions<AccountsTransactionsContext> options) : base(options)
		{

		}

		#region DBSet
		public virtual DbSet<Transaction>? Transactions { get; set; }
		public virtual DbSet<Order>? Orders { get; set; }
		public virtual DbSet<FeedBack>? Feedbacks { get; set; }
		#endregion

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
			SeedRoles(builder);
		}
		private void SeedRoles(ModelBuilder builder)
		{
			// Always create the roles
			builder.Entity<IdentityRole>().HasData
			(
				new IdentityRole() { Id = "1" , Name = "admin" , NormalizedName = "ADMIN" } ,
				new IdentityRole() { Id = "2" , Name = "manager" , NormalizedName = "MANAGER" } ,
				new IdentityRole() { Id = "3" , Name = "staff" , NormalizedName = "STAFF" } ,
				new IdentityRole() { Id = "4" , Name = "customer" , NormalizedName = "CUSTOMER" }
			);

			// Create the admin user and assign the "Admin" role
			var adminUser = new User
			{
				UserName = "admin" ,
				NormalizedUserName = "ADMIN" ,
				Email = "admin@gmail.com" ,
				NormalizedEmail = "ADMIN@GMAIL.COM" ,
				EmailConfirmed = true ,
				FirstName = "admin" ,
				LastName = "No 1" ,
				LockoutEnabled = true ,
				AccessFailedCount = 0 ,
				Gender = Enums.UserGender.Male ,
			};

			var password = "Admin123@";

			builder.Entity<User>().HasData(adminUser);

			var hasher = new PasswordHasher<User>();
			adminUser.PasswordHash = hasher.HashPassword(adminUser , password);

			builder.Entity<IdentityUserRole<string>>().HasData(
				new IdentityUserRole<string>
				{
					RoleId = "1" , // "Admin" role
					UserId = adminUser.Id
				}
			);
		}
	}
}
