using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace RegionOrebroLan.Web.Authentication.Certificate.UnitTests.Helpers
{
	public class CertificateBuilder
	{
		#region Fields

		// ReSharper disable PossibleNullReferenceException
		private static readonly string _projectDirectoryPath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
		// ReSharper restore PossibleNullReferenceException

		#endregion

		#region Properties

		public virtual bool? Archived { get; set; }
		public virtual string FriendlyName { get; set; }
		public virtual bool PrivateKey { get; set; }
		protected internal virtual string ProjectDirectoryPath => _projectDirectoryPath;

		#endregion

		#region Methods

		public virtual X509Certificate2 Build()
		{
			var certificateFileName = $"Unit-test-certificate.{(this.PrivateKey ? "pfx" : "cer")}";
			var certificateFilePath = Path.Combine(this.ProjectDirectoryPath, "Resources", "Certificates", certificateFileName);

			var certificate = new X509Certificate2(certificateFilePath, "password");

			if(this.Archived != null)
				certificate.Archived = this.Archived.Value;

			if(this.FriendlyName != null)
				certificate.FriendlyName = this.FriendlyName;

			return certificate;
		}

		#endregion
	}
}