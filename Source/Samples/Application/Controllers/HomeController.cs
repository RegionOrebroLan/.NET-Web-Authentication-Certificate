using System;
using System.Threading.Tasks;
using Application.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers
{
	public class HomeController : Controller
	{
		#region Constructors

		public HomeController(IAuthenticationSchemeProvider authenticationSchemeProvider)
		{
			this.AuthenticationSchemeProvider = authenticationSchemeProvider ?? throw new ArgumentNullException(nameof(authenticationSchemeProvider));
		}

		#endregion

		#region Properties

		protected internal virtual IAuthenticationSchemeProvider AuthenticationSchemeProvider { get; }

		#endregion

		#region Methods

		public virtual async Task<IActionResult> Index()
		{
			return await Task.FromResult(this.View());
		}

		public virtual async Task<IActionResult> Schemes()
		{
			var model = new HomeViewModel();

			foreach(var authenticationScheme in await this.AuthenticationSchemeProvider.GetAllSchemesAsync())
			{
				model.AuthenticationSchemes.Add(authenticationScheme);
			}

			return await Task.FromResult(this.View(model));
		}

		#endregion
	}
}