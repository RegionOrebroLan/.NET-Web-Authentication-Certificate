using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.Web.Authentication.Certificate.Claims;

namespace IntegrationTests.Claims
{
	[TestClass]
	public class CertificatePrincipalFactoryTest
	{
		#region Fields

		private static readonly string _certificateDirectoryPath = Path.Combine(Global.ProjectDirectoryPath, "Claims", "Resources", "Certificates");

		#endregion

		#region Properties

		protected internal virtual string CertificateDirectoryPath => _certificateDirectoryPath;
		protected internal virtual string ClientCertificatePath => Path.Combine(this.CertificateDirectoryPath, "Integration-test client-certificate.cer");
		protected internal virtual string ClientCertificateWithEmailAndUpnPath => Path.Combine(this.CertificateDirectoryPath, "Integration-test client-certificate with email and UPN.cer");
		protected internal virtual string RootCertificatePath => Path.Combine(this.CertificateDirectoryPath, "Integration-test Root CA.cer");

		#endregion

		#region Methods

		[TestMethod]
		public void Create_1()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var certificate = new X509Certificate2(this.ClientCertificatePath))
			{
				var certificatePrincipal = new CertificatePrincipalFactory().Create("Certificate", certificate, "Custom");

				Assert.IsNotNull(certificatePrincipal);
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		public void Create_2()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var certificate = new X509Certificate2(this.ClientCertificateWithEmailAndUpnPath))
			{
				var certificatePrincipal = new CertificatePrincipalFactory().Create("Certificate", certificate, null);

				Assert.IsNotNull(certificatePrincipal);
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		public void Create_3()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var certificate = new X509Certificate2(this.RootCertificatePath))
			{
				var certificatePrincipal = new CertificatePrincipalFactory().Create("Certificate", certificate, null);

				Assert.IsNotNull(certificatePrincipal);
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		#endregion
	}
}