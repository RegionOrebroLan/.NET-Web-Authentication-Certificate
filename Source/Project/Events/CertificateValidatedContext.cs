using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace RegionOrebroLan.Web.Authentication.Certificate.Events
{
	[CLSCompliant(false)]
	public class CertificateValidatedContext : ResultContext<CertificateAuthenticationOptions>
	{
		#region Constructors

		public CertificateValidatedContext(HttpContext context, CertificateAuthenticationOptions options, AuthenticationScheme scheme) : base(context, scheme, options) { }

		#endregion

		#region Properties

		public virtual X509Certificate2 ClientCertificate { get; set; }

		#endregion
	}
}