using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RegionOrebroLan.Web.Authentication.Certificate;

namespace IntegrationTests
{
	[TestClass]
	public class AuthenticationBuilderExtensionTest
	{
		#region Methods

		[TestMethod]
		public void AddCertificate_ShouldAddTheCorrectAmountOfServiceDescriptors()
		{
			var authenticationBuilder = new ServiceCollection().AddAuthentication();

			Assert.AreEqual(24, authenticationBuilder.Services.Count);

			authenticationBuilder.AddCertificate();

			Assert.AreEqual(35, authenticationBuilder.Services.Count);
		}

		[TestMethod]
		public void AddCertificate_ShouldMakeTheCertificateAuthenticationHandlerAvailable()
		{
			var hostEnvironmentMock = new Mock<IHostEnvironment>();
			hostEnvironmentMock.Setup(hostEnvironment => hostEnvironment.ContentRootPath).Returns(Global.ProjectDirectoryPath);

			var services = new ServiceCollection().AddAuthentication().AddCertificate().Services;
			services.AddSingleton(hostEnvironmentMock.Object);
			services.AddSingleton(Mock.Of<ILoggerFactory>());
			var serviceProvider = services.BuildServiceProvider();
			var httpContext = new DefaultHttpContext
			{
				RequestServices = serviceProvider
			};

			var authenticationHandler = serviceProvider.GetRequiredService<IAuthenticationHandlerProvider>().GetHandlerAsync(httpContext, CertificateAuthenticationDefaults.AuthenticationScheme).Result;

			Assert.IsNotNull(authenticationHandler);
			Assert.IsTrue(authenticationHandler is CertificateAuthenticationHandler);
		}

		#endregion
	}
}