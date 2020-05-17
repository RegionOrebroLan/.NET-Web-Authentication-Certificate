using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Security.Cryptography;
using RegionOrebroLan.Security.Cryptography.Validation;
using RegionOrebroLan.Security.Cryptography.Validation.Configuration;
using RegionOrebroLan.Web.Authentication.Certificate.Claims;
using RegionOrebroLan.Web.Authentication.Certificate.Configuration;

namespace RegionOrebroLan.Web.Authentication.Certificate
{
	[CLSCompliant(false)]
	public static class AuthenticationBuilderExtension
	{
		#region Methods

		public static AuthenticationBuilder AddCertificate(this AuthenticationBuilder builder)
		{
			return builder.AddCertificate(CertificateAuthenticationDefaults.AuthenticationScheme);
		}

		public static AuthenticationBuilder AddCertificate(this AuthenticationBuilder authenticationBuilder, string authenticationScheme)
		{
			return authenticationBuilder.AddCertificate(authenticationScheme, null, null);
		}

		public static AuthenticationBuilder AddCertificate(this AuthenticationBuilder authenticationBuilder, Action<CertificateAuthenticationOptions> optionsAction)
		{
			return authenticationBuilder.AddCertificate(CertificateAuthenticationDefaults.AuthenticationScheme, optionsAction);
		}

		public static AuthenticationBuilder AddCertificate(this AuthenticationBuilder authenticationBuilder, string authenticationScheme, string displayName)
		{
			return authenticationBuilder.AddCertificate(authenticationScheme, displayName, null);
		}

		public static AuthenticationBuilder AddCertificate(this AuthenticationBuilder authenticationBuilder, string authenticationScheme, Action<CertificateAuthenticationOptions> optionsAction)
		{
			return authenticationBuilder.AddCertificate(authenticationScheme, null, optionsAction);
		}

		public static AuthenticationBuilder AddCertificate(this AuthenticationBuilder authenticationBuilder, string authenticationScheme, string displayName, Action<CertificateAuthenticationOptions> optionsAction)
		{
			if(authenticationBuilder == null)
				throw new ArgumentNullException(nameof(authenticationBuilder));

			authenticationBuilder.Services.TryAddSingleton(AppDomain.CurrentDomain);
			authenticationBuilder.Services.TryAddSingleton<FileCertificateResolver>();
			authenticationBuilder.Services.TryAddSingleton<IApplicationDomain, AppDomainWrapper>();
			authenticationBuilder.Services.TryAddSingleton<ICertificatePrincipalFactory, CertificatePrincipalFactory>();
			authenticationBuilder.Services.TryAddSingleton<ICertificateResolver, CertificateResolver>();
			authenticationBuilder.Services.TryAddSingleton<ICertificateValidator, CertificateValidator>();
			authenticationBuilder.Services.TryAddSingleton<IPostConfigureOptions<CertificateAuthenticationOptions>, PostConfigureCertificateAuthenticationOptions>();
			authenticationBuilder.Services.TryAddSingleton<IPostConfigureOptions<CertificateValidatorOptions>, PostConfigureCertificateValidatorOptions>();
			authenticationBuilder.Services.TryAddSingleton<StoreCertificateResolver>();

			return authenticationBuilder.AddScheme<CertificateAuthenticationOptions, CertificateAuthenticationHandler>(authenticationScheme, displayName, optionsAction);
		}

		#endregion
	}
}