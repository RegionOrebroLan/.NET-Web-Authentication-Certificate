using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Security.Cryptography.Validation;
using RegionOrebroLan.Web.Authentication.Certificate.Claims;

namespace RegionOrebroLan.Web.Authentication.Certificate.UnitTests.Mocks
{
	/// <summary>
	/// A mock of CertificateAuthenticationHandler so we can call protected members.
	/// </summary>
	public class CertificateAuthenticationHandlerMock : CertificateAuthenticationHandler
	{
		#region Constructors

		public CertificateAuthenticationHandlerMock(ICertificatePrincipalFactory certificatePrincipalFactory, ICertificateValidator certificateValidator, ILoggerFactory loggerFactory, IOptionsMonitor<CertificateAuthenticationOptions> options, ISystemClock systemClock, UrlEncoder urlEncoder) : base(certificatePrincipalFactory, certificateValidator, loggerFactory, options, systemClock, urlEncoder) { }

		#endregion

		#region Methods

		public new virtual async Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			return await base.HandleAuthenticateAsync().ConfigureAwait(false);
		}

		#endregion
	}
}