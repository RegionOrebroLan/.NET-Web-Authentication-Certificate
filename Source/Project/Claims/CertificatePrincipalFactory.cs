using System;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using RegionOrebroLan.Security.Claims;
using RegionOrebroLan.Security.Claims.Extensions;

namespace RegionOrebroLan.Web.Authentication.Certificate.Claims
{
	public class CertificatePrincipalFactory : ICertificatePrincipalFactory
	{
		#region Methods

		protected internal virtual void BuildClaims(X509Certificate2 certificate, IClaimBuilderCollection claimsBuilder)
		{
			if(certificate == null)
				throw new ArgumentNullException(nameof(certificate));

			if(claimsBuilder == null)
				throw new ArgumentNullException(nameof(claimsBuilder));

			claimsBuilder.Add(ClaimTypes.Dns, certificate.GetNameInfo(X509NameType.DnsName, false));
			claimsBuilder.Add(ClaimTypes.Email, certificate.GetNameInfo(X509NameType.EmailName, false));
			claimsBuilder.Add("issuer", certificate.Issuer);
			claimsBuilder.Add(ClaimTypes.Name, certificate.GetNameInfo(X509NameType.SimpleName, false));
			claimsBuilder.Add(ClaimTypes.NameIdentifier, certificate.Thumbprint, ClaimValueTypes.Base64Binary);
			claimsBuilder.Add(ClaimTypes.SerialNumber, certificate.SerialNumber);
			claimsBuilder.Add(ClaimTypes.Thumbprint, certificate.Thumbprint, ClaimValueTypes.Base64Binary);
			claimsBuilder.Add(ClaimTypes.Upn, certificate.GetNameInfo(X509NameType.UpnName, false));
			claimsBuilder.Add(ClaimTypes.Uri, certificate.GetNameInfo(X509NameType.UrlName, false));
			claimsBuilder.Add(ClaimTypes.X500DistinguishedName, certificate.SubjectName.Name);
		}

		public virtual ClaimsPrincipal Create(string authenticationType, X509Certificate2 certificate, string claimsIssuer = null)
		{
			if(certificate == null)
				throw new ArgumentNullException(nameof(certificate));

			var claimsBuilder = this.CreateClaimsBuilder(authenticationType, certificate, claimsIssuer);

			this.BuildClaims(certificate, claimsBuilder);

			return new ClaimsPrincipal(new ClaimsIdentity(claimsBuilder.Build(), authenticationType));
		}

		protected internal virtual IClaimBuilderCollection CreateClaimsBuilder(string authenticationType, X509Certificate2 certificate, string claimsIssuer)
		{
			return new ClaimBuilderCollection
			{
				DefaultIssuer = claimsIssuer ?? authenticationType,
				DefaultOriginalIssuer = certificate?.Issuer
			};
		}

		#endregion
	}
}