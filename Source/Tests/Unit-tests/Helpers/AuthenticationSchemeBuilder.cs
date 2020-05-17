using System;
using Microsoft.AspNetCore.Authentication;

namespace RegionOrebroLan.Web.Authentication.Certificate.UnitTests.Helpers
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