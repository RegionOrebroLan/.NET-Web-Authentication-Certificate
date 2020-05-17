using System;
using Microsoft.AspNetCore.Authentication;
using RegionOrebroLan.Security.Cryptography.Validation.Configuration;

namespace RegionOrebroLan.Web.Authentication.Certificate
{
	[CLSCompliant(false)]
	public class CertificateAuthenticationOptions : AuthenticationSchemeOptions
	{
		#region Properties

		public virtual CertificateValidatorOptions Validator { get; set; } = new CertificateValidatorOptions();

		#endregion
	}
}