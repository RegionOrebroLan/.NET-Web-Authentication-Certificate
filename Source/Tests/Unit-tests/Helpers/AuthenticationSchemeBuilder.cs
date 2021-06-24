using System;
using Microsoft.AspNetCore.Authentication;
using RegionOrebroLan.Web.Authentication.Certificate;

namespace UnitTests.Helpers
{
	public class AuthenticationSchemeBuilder
	{
		#region Properties

		public virtual string DisplayName { get; set; }
		public virtual Type HandlerType { get; set; } = typeof(CertificateAuthenticationHandler);
		public virtual string Name { get; set; } = "Certificate";

		#endregion

		#region Methods

		public virtual AuthenticationScheme Build()
		{
			return new AuthenticationScheme(this.Name, this.DisplayName, this.HandlerType);
		}

		#endregion
	}
}