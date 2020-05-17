using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Web.Authentication.Certificate;

namespace Application.Controllers
{
	[Authorize]
	public class AccountController : Controller
	{
		#region Constructors

		public AccountController(IOptions<AuthenticationOptions> authenticationOptions, IAuthenticationSchemeProvider authenticationSchemeProvider)
		{
			this.AuthenticationOptions = authenticationOptions ?? throw new ArgumentNullException(nameof(authenticationOptions));
			this.AuthenticationSchemeProvider = authenticationSchemeProvider ?? throw new ArgumentNullException(nameof(authenticationSchemeProvider));
		}

		#endregion

		#region Properties

		protected internal virtual IOptions<AuthenticationOptions> AuthenticationOptions { get; }
		protected internal virtual IAuthenticationSchemeProvider AuthenticationSchemeProvider { get; }

		#endregion

		#region Methods

		public virtual async Task<IActionResult> Index()
		{
			return await Task.FromResult(this.View());
		}

		[AllowAnonymous]
		public virtual async Task<IActionResult> SignIn(string returnUrl)
		{
			if(string.IsNullOrEmpty(returnUrl))
				returnUrl = "~/";

			if(!this.Url.IsLocalUrl(returnUrl))
				throw new Exception("Invalid return-url.");

			var model = new SignInViewModel
			{
				ReturnUrl = returnUrl
			};

			var authenticationSchemes = (await this.AuthenticationSchemeProvider.GetAllSchemesAsync())
				.Where(authenticationScheme => authenticationScheme.HandlerType == typeof(CertificateAuthenticationHandler))
				.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase);

			foreach(var authenticationScheme in authenticationSchemes)
			{
				model.AuthenticationSchemes.Add(authenticationScheme);
			}

			return this.View(model);
		}

		[AllowAnonymous]
		public virtual async Task<IActionResult> SignOut(string signOutId)
		{
			if(this.User.Identity.IsAuthenticated)
				return this.View(new SignOutViewModel {Form = {Id = signOutId}});

			return await Task.FromResult(this.View("SignedOut"));
		}

		[AllowAnonymous]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> SignOut(SignOutForm form)
		{
			if(this.User.Identity.IsAuthenticated)
				await this.HttpContext.SignOutAsync();

			return this.View("SignedOut");
		}

		#endregion
	}
}