using System;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Security.Cryptography.Validation.Configuration;

namespace RegionOrebroLan.Web.Authentication.Certificate.Configuration
{
	[CLSCompliant(false)]
	public class PostConfigureCertificateAuthenticationOptions : IPostConfigureOptions<CertificateAuthenticationOptions>
	{
		#region Constructors

		public PostConfigureCertificateAuthenticationOptions(IPostConfigureOptions<CertificateValidatorOptions> postConfigureCertificateValidatorOptions)
		{
			this.PostConfigureCertificateValidatorOptions = postConfigureCertificateValidatorOptions ?? throw new ArgumentNullException(nameof(postConfigureCertificateValidatorOptions));
		}

		#endregion

		#region Properties

		protected internal virtual IPostConfigureOptions<CertificateValidatorOptions> PostConfigureCertificateValidatorOptions { get; }

		#endregion

		#region Methods

		public virtual void PostConfigure(string name, CertificateAuthenticationOptions options)
		{
			this.PostConfigureCertificateValidatorOptions.PostConfigure(name, options?.Validator);
		}

		#endregion
	}
}