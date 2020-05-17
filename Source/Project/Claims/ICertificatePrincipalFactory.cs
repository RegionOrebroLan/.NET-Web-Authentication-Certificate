using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace RegionOrebroLan.Web.Authentication.Certificate.Claims
{
	public interface ICertificatePrincipalFactory
	{
		#region Methods

		ClaimsPrincipal Create(string authenticationType, X509Certificate2 certificate, string claimsIssuer = null);

		#endregion
	}
}