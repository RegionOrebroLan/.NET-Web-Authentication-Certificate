using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RegionOrebroLan.Web.Authentication.Certificate;

namespace Application
{
	public class Startup
	{
		#region Constructors

		public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
		{
			this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.HostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
		}

		#endregion

		#region Properties

		public virtual IConfiguration Configuration { get; }
		public virtual IHostEnvironment HostEnvironment { get; }

		#endregion

		#region Methods

		public virtual void Configure(IApplicationBuilder applicationBuilder)
		{
			applicationBuilder
				.UseDeveloperExceptionPage()
				.UseStaticFiles()
				.UseRouting()
				.UseAuthentication()
				.UseAuthorization()
				.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
		}

		public virtual void ConfigureServices(IServiceCollection services)
		{
			var authenticationSection = this.Configuration.GetSection("Authentication");
			var authenticationOptions = new AuthenticationOptions();
			authenticationSection.Bind(authenticationOptions);

			services.AddAuthentication(options => { authenticationSection.Bind(options); })
				.AddCookie(authenticationOptions.DefaultScheme, options =>
				{
					options.LoginPath = "/Account/SignIn/";
					options.LogoutPath = "/Account/SignOut/";
				})
				.AddCookie(authenticationOptions.DefaultSignInScheme)
				.AddCertificate(options => { this.Configuration.GetSection("Authentication:Certificate").Bind(options); });

			services.AddControllersWithViews();
		}

		#endregion
	}
}