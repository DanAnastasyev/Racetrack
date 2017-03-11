using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Racetrack.Models {
	// You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
	public class ApplicationUser : IdentityUser {
		public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager) {
			// Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
			var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
			// Add custom user claims here
			return userIdentity;
		}
	}

	public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {
		public ApplicationDbContext()
			: base("DefaultConnection", false) {}

		public static ApplicationDbContext Create() {
			return new ApplicationDbContext();
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder) {
			base.OnModelCreating(modelBuilder); // This needs to go before the other rules!
			
		   // In order to support multiple user names 
		   // I replaced unique index of UserNameIndex to non-unique
		   modelBuilder
		   .Entity<ApplicationUser>()
		   .Property(c => c.UserName)
		   .HasColumnAnnotation(
			   "Index",
			   new IndexAnnotation(
			   new IndexAttribute("UserNameIndex") {
				   IsUnique = false
			   }));
		}

		/// <summary>
		///     Override 'ValidateEntity' to support multiple users with the same name
		/// </summary>
		/// <param name="entityEntry"></param>
		/// <param name="items"></param>
		/// <returns></returns>
		protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry,
			IDictionary<object, object> items) {
			// call validate and check results 
			var result = base.ValidateEntity(entityEntry, items);

			if (result.ValidationErrors.Any(err => err.PropertyName.Equals("User"))) {
				// Yes I know! Next code looks not good, because I rely on internal messages of Identity 2, but I should track here only error message instead of rewriting the whole IdentityDbContext

				var duplicateUserNameError =
					result.ValidationErrors
					.FirstOrDefault(
					err =>
						Regex.IsMatch(
							err.ErrorMessage,
							@"Name\s+(.+)is\s+already\s+taken",
							RegexOptions.IgnoreCase));

				if (null != duplicateUserNameError) {
					result.ValidationErrors.Remove(duplicateUserNameError);
				}
			}

			return result;
		}
	}
}