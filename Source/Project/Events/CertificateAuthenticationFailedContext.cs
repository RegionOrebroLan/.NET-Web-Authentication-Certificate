using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace RegionOrebroLan.Web.Authentication.Certificate.Events
{
	[CLSCompliant(false)]
	public class CertificateAuthenticationFailedContext : ResultContext<CertificateAuthenticationOptions>
	{
		#region Constructors

		public CertificateAuthenticationFailedContext(HttpContext context, CertificateAuthenticationOptions options, AuthenticationScheme scheme) : base(context, scheme, options) { }

		#endregion

		#region Properties

		public virtual Exception Exception { get; set; }

		#endregion
	}
}