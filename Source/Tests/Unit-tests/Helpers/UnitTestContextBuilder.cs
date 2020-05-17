using System;
using System.Text.Encodings.Web;
using RegionOrebroLan.Web.Authentication.Certificate.UnitTests.Mocks;

namespace RegionOrebroLan.Web.Authentication.Certificate.UnitTests.Helpers
{
	public class UnitTestContextBuilder
	{
		#region Properties

		public virtual AuthenticationSchemeBuilder AuthenticationSchemeBuilder { get; } = new AuthenticationSchemeBuilder();
		public virtual Action<CertificateAuthenticationOptions> CertificateAuthenticationOptionsConfigurer { get; set; }
		public virtual HttpContextMock HttpContext { get; set; } = new HttpContextMock();
		public virtual LoggerFactoryMock LoggerFactory { get; set; } = new LoggerFactoryMock();
		public virtual SystemClockMock SystemClock { get; } = new SystemClockMock();
		public virtual UrlEncoder UrlEncoder { get; set; } = UrlEncoder.Default;

		#endregion

		#region Methods

		public virtual UnitTestContext Build()
		{
			return new UnitTestContext(this.AuthenticationSchemeBuilder.Build(), this.CertificateAuthenticationOptionsConfigurer, this.HttpContext, this.LoggerFactory, this.SystemClock, this.UrlEncoder);
		}

		#endregion
	}
}