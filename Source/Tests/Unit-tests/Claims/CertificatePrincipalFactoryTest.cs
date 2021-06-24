using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.Web.Authentication.Certificate.Claims;

namespace UnitTests.Claims
{
	[TestClass]
	public class CertificatePrincipalFactoryTest
	{
		#region Methods

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Create_IfTheCertificateParameterIsNull_ShouldThrowAnArgumentNullException()
		{
			try
			{
				new CertificatePrincipalFactory().Create("Authentication-type", null);
			}
			catch(ArgumentNullException argumentNullException)
			{
				if(string.Equals(argumentNullException.ParamName, "certificate", StringComparison.Ordinal))
					throw;
			}
		}

		#endregion
	}
}