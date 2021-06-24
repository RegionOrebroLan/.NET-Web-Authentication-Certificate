using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using RegionOrebroLan.Web.Authentication.Certificate;
using UnitTests.Mocks;

namespace UnitTests.Helpers
{
	public class UnitTestContext
	{
		#region Constructors

		public UnitTestContext(AuthenticationScheme authenticationScheme, Action<CertificateAuthenticationOptions> certificateAuthenticationOptionsConfigurer, HttpContextMock httpContext, LoggerFactoryMock loggerFactory, SystemClockMock systemClock, UrlEncoder urlEncoder)
		{
			this.AuthenticationScheme = authenticationScheme ?? throw new ArgumentNullException(nameof(authenticationScheme));
			this.CertificateAuthenticationOptionsMonitor = new OptionsMonitorMock<CertificateAuthenticationOptions>(certificateAuthenticationOptionsConfigurer);
			this.HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
			this.LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			this.SystemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
			this.UrlEncoder = urlEncoder ?? throw new ArgumentNullException(nameof(urlEncoder));
		}

		#endregion

		#region Properties

		public virtual AuthenticationScheme AuthenticationScheme { get; }
		public virtual OptionsMonitorMock<CertificateAuthenticationOptions> CertificateAuthenticationOptionsMonitor { get; }
		public virtual HttpContextMock HttpContext { get; }
		public virtual LoggerFactoryMock LoggerFactory { get; }
		public virtual IEnumerable<LogMock> Logs => this.LoggerFactory.Logs;
		public virtual SystemClockMock SystemClock { get; }
		public virtual UrlEncoder UrlEncoder { get; }

		#endregion
	}
}