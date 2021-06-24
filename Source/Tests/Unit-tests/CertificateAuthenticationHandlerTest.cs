using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RegionOrebroLan.Security.Cryptography.Validation;
using RegionOrebroLan.Security.Cryptography.Validation.Configuration;
using RegionOrebroLan.Validation;
using RegionOrebroLan.Web.Authentication.Certificate.Claims;
using UnitTests.Helpers;
using UnitTests.Mocks;

namespace UnitTests
{
	[TestClass]
	public class CertificateAuthenticationHandlerTest
	{
		#region Fields

		private const string _authenticationExceptionMessage = "Could not handle authentication.";
		private static readonly string _invalidClientCertificateMessage = $"Client-certificate \"CN=Unit-test-certificate\" failed validation. Exceptions:{Environment.NewLine} - System.InvalidOperationException: First{Environment.NewLine} - System.InvalidOperationException: Second{Environment.NewLine} - System.InvalidOperationException: Third";
		private const string _noClientCertificateLogMessage = "The request does not contain a client-certificate.";
		private const string _noHttpsLogMessage = "The request is not https. Client-certificates are only available over https.";

		#endregion

		#region Methods

		protected internal virtual CertificateAuthenticationHandlerMock CreateAuthenticationHandler(UnitTestContext unitTestContext)
		{
			return this.CreateAuthenticationHandler(Mock.Of<ICertificatePrincipalFactory>(), Mock.Of<ICertificateValidator>(), unitTestContext);
		}

		protected internal virtual CertificateAuthenticationHandlerMock CreateAuthenticationHandler(ICertificatePrincipalFactory certificatePrincipalFactory, UnitTestContext unitTestContext)
		{
			return this.CreateAuthenticationHandler(certificatePrincipalFactory, Mock.Of<ICertificateValidator>(), unitTestContext);
		}

		protected internal virtual CertificateAuthenticationHandlerMock CreateAuthenticationHandler(ICertificateValidator certificateValidator, UnitTestContext unitTestContext)
		{
			return this.CreateAuthenticationHandler(Mock.Of<ICertificatePrincipalFactory>(), certificateValidator, unitTestContext);
		}

		protected internal virtual CertificateAuthenticationHandlerMock CreateAuthenticationHandler(ICertificatePrincipalFactory certificatePrincipalFactory, ICertificateValidator certificateValidator, UnitTestContext unitTestContext)
		{
			if(certificatePrincipalFactory == null)
				throw new ArgumentNullException(nameof(certificatePrincipalFactory));

			if(certificateValidator == null)
				throw new ArgumentNullException(nameof(certificateValidator));

			if(unitTestContext == null)
				throw new ArgumentNullException(nameof(unitTestContext));

			var authenticationHandler = new CertificateAuthenticationHandlerMock(certificatePrincipalFactory, certificateValidator, unitTestContext.LoggerFactory, unitTestContext.CertificateAuthenticationOptionsMonitor, unitTestContext.SystemClock, unitTestContext.UrlEncoder);

			authenticationHandler.InitializeAsync(unitTestContext.AuthenticationScheme, unitTestContext.HttpContext);

			return authenticationHandler;
		}

		protected internal virtual IValidationResult CreateInvalidValidationResult()
		{
			var validationResultMock = new Mock<IValidationResult>();

			validationResultMock.Setup(validationResult => validationResult.Exceptions)
				.Returns(new List<Exception>
					{
						new InvalidOperationException("First"),
						new InvalidOperationException("Second"),
						new InvalidOperationException("Third")
					}
				);
			validationResultMock.Setup(validationResult => validationResult.Valid).Returns(false);

			return validationResultMock.Object;
		}

		protected internal virtual IValidationResult CreateValidValidationResult()
		{
			var validationResultMock = new Mock<IValidationResult>();

			validationResultMock.Setup(validationResult => validationResult.Exceptions).Returns(new List<Exception>());
			validationResultMock.Setup(validationResult => validationResult.Valid).Returns(true);

			return validationResultMock.Object;
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheCertificateIsValid_And_IfTheResultFromTheCertificateValidatedContextIsNotNull_ShouldReturnTheResultFromTheCertificateValidatedContext()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var clientCertificate = new CertificateBuilder().Build())
			{
				var unitTestContext = new UnitTestContextBuilder
				{
					HttpContext =
					{
						Connection =
						{
							ClientCertificate = clientCertificate
						},
						Request =
						{
							IsHttps = true
						}
					}
				}.Build();

				Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
				Assert.IsNotNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);

				var certificatePrincipalFactoryMock = new Mock<ICertificatePrincipalFactory>();
				certificatePrincipalFactoryMock
					.Setup(certificatePrincipalFactory => certificatePrincipalFactory.Create(It.IsAny<string>(), It.IsAny<X509Certificate2>(), It.IsAny<string>()))
					.Returns(new ClaimsPrincipal(new ClaimsIdentity()));

				var certificateValidatorMock = new Mock<ICertificateValidator>();
				certificateValidatorMock
					.Setup(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()))
					.Returns(Task.FromResult(this.CreateValidValidationResult()));

				var certificateAuthenticationHandler = this.CreateAuthenticationHandler(certificatePrincipalFactoryMock.Object, certificateValidatorMock.Object, unitTestContext);

				var identifier = Guid.NewGuid().ToString();

				certificateAuthenticationHandler.Events.OnCertificateValidated = async context =>
				{
					context.Properties.Items.Add(identifier, identifier);
					context.Success();

					await Task.CompletedTask.ConfigureAwait(false);
				};

				var authenticateResult = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;

				Assert.IsNotNull(authenticateResult);
				Assert.IsTrue(authenticateResult.Succeeded);
				Assert.IsTrue(authenticateResult.Properties.Items.ContainsKey(identifier));
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheCertificateIsValid_And_IfTheResultFromTheCertificateValidatedContextIsNull_ShouldReturnAnAuthenticateResultWhereSuccessIsTrue()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var clientCertificate = new CertificateBuilder().Build())
			{
				var unitTestContext = new UnitTestContextBuilder
				{
					HttpContext =
					{
						Connection =
						{
							ClientCertificate = clientCertificate
						},
						Request =
						{
							IsHttps = true
						}
					}
				}.Build();

				Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
				Assert.IsNotNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);

				var certificatePrincipalFactoryMock = new Mock<ICertificatePrincipalFactory>();
				certificatePrincipalFactoryMock
					.Setup(certificatePrincipalFactory => certificatePrincipalFactory.Create(It.IsAny<string>(), It.IsAny<X509Certificate2>(), It.IsAny<string>()))
					.Returns(new ClaimsPrincipal(new ClaimsIdentity()));

				var certificateValidatorMock = new Mock<ICertificateValidator>();
				certificateValidatorMock
					.Setup(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()))
					.Returns(Task.FromResult(this.CreateValidValidationResult()));

				var certificateAuthenticationHandler = this.CreateAuthenticationHandler(certificatePrincipalFactoryMock.Object, certificateValidatorMock.Object, unitTestContext);

				var authenticateResult = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;

				Assert.IsNotNull(authenticateResult);
				Assert.IsTrue(authenticateResult.Succeeded);
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheCertificatePrincipalFactoryThrowsAnException_And_IfLoggingIsDisabled_ShouldNotLogAnError()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var clientCertificate = new CertificateBuilder().Build())
			{
				var unitTestContext = new UnitTestContextBuilder
				{
					HttpContext =
					{
						Connection =
						{
							ClientCertificate = clientCertificate
						},
						Request =
						{
							IsHttps = true
						}
					},
					LoggerFactory =
					{
						Enabled = false
					}
				}.Build();

				Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
				Assert.IsNotNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);
				Assert.IsFalse(unitTestContext.LoggerFactory.Enabled);
				Assert.IsFalse(unitTestContext.Logs.Any());

				var certificatePrincipalFactoryException = new InvalidOperationException("Exception from certificate-principal-factory.");

				var certificatePrincipalFactoryMock = new Mock<ICertificatePrincipalFactory>();
				certificatePrincipalFactoryMock
					.Setup(certificatePrincipalFactory => certificatePrincipalFactory.Create(It.IsAny<string>(), It.IsAny<X509Certificate2>(), It.IsAny<string>()))
					.Throws(certificatePrincipalFactoryException);

				var certificateValidatorMock = new Mock<ICertificateValidator>();
				certificateValidatorMock
					.Setup(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()))
					.Returns(Task.FromResult(this.CreateValidValidationResult()));

				var certificateAuthenticationHandler = this.CreateAuthenticationHandler(certificatePrincipalFactoryMock.Object, certificateValidatorMock.Object, unitTestContext);

				certificateAuthenticationHandler.Events.OnAuthenticationFailed = async context =>
				{
					context.Fail(context.Exception);

					await Task.CompletedTask.ConfigureAwait(false);
				};

				_ = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;

				var log = unitTestContext.Logs.FirstOrDefault(item => string.Equals(item.Message, _authenticationExceptionMessage, StringComparison.Ordinal));
				Assert.IsNull(log);
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheCertificatePrincipalFactoryThrowsAnException_And_IfLoggingIsEnabled_ShouldLogAnError()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var clientCertificate = new CertificateBuilder().Build())
			{
				var unitTestContext = new UnitTestContextBuilder
				{
					HttpContext =
					{
						Connection =
						{
							ClientCertificate = clientCertificate
						},
						Request =
						{
							IsHttps = true
						}
					}
				}.Build();

				Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
				Assert.IsNotNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);
				Assert.IsTrue(unitTestContext.LoggerFactory.Enabled);
				Assert.IsFalse(unitTestContext.Logs.Any());

				var certificatePrincipalFactoryException = new InvalidOperationException("Exception from certificate-principal-factory.");

				var certificatePrincipalFactoryMock = new Mock<ICertificatePrincipalFactory>();
				certificatePrincipalFactoryMock
					.Setup(certificatePrincipalFactory => certificatePrincipalFactory.Create(It.IsAny<string>(), It.IsAny<X509Certificate2>(), It.IsAny<string>()))
					.Throws(certificatePrincipalFactoryException);

				var certificateValidatorMock = new Mock<ICertificateValidator>();
				certificateValidatorMock
					.Setup(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()))
					.Returns(Task.FromResult(this.CreateValidValidationResult()));

				var certificateAuthenticationHandler = this.CreateAuthenticationHandler(certificatePrincipalFactoryMock.Object, certificateValidatorMock.Object, unitTestContext);

				certificateAuthenticationHandler.Events.OnAuthenticationFailed = async context =>
				{
					context.Fail(context.Exception);

					await Task.CompletedTask.ConfigureAwait(false);
				};

				_ = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;

				var log = unitTestContext.Logs.FirstOrDefault(item => string.Equals(item.Message, _authenticationExceptionMessage, StringComparison.Ordinal));
				Assert.IsNotNull(log);
				Assert.AreEqual(LogLevel.Error, log.LogLevel);
				Assert.IsTrue(ReferenceEquals(certificatePrincipalFactoryException, log.Exception));
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheCertificatePrincipalFactoryThrowsAnException_And_IfTheResultFromTheCertificateAuthenticationFailedContextIsNotNull_ShouldReturnTheResultFromTheCertificateAuthenticationFailedContext()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var clientCertificate = new CertificateBuilder().Build())
			{
				var unitTestContext = new UnitTestContextBuilder
				{
					HttpContext =
					{
						Connection =
						{
							ClientCertificate = clientCertificate
						},
						Request =
						{
							IsHttps = true
						}
					}
				}.Build();

				Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
				Assert.IsNotNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);

				var certificatePrincipalFactoryException = new InvalidOperationException("Exception from certificate-principal-factory.");

				var certificatePrincipalFactoryMock = new Mock<ICertificatePrincipalFactory>();
				certificatePrincipalFactoryMock
					.Setup(certificatePrincipalFactory => certificatePrincipalFactory.Create(It.IsAny<string>(), It.IsAny<X509Certificate2>(), It.IsAny<string>()))
					.Throws(certificatePrincipalFactoryException);

				var certificateValidatorMock = new Mock<ICertificateValidator>();
				certificateValidatorMock
					.Setup(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()))
					.Returns(Task.FromResult(this.CreateValidValidationResult()));

				var certificateAuthenticationHandler = this.CreateAuthenticationHandler(certificatePrincipalFactoryMock.Object, certificateValidatorMock.Object, unitTestContext);

				certificateAuthenticationHandler.Events.OnAuthenticationFailed = async context =>
				{
					context.Fail(context.Exception);

					await Task.CompletedTask.ConfigureAwait(false);
				};

				var authenticateResult = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;

				Assert.IsNotNull(authenticateResult);
				Assert.IsNotNull(authenticateResult.Failure);
				Assert.AreEqual(_authenticationExceptionMessage, authenticateResult.Failure.Message);
				Assert.AreEqual(certificatePrincipalFactoryException, authenticateResult.Failure.InnerException);
				Assert.IsFalse(authenticateResult.None);
				Assert.IsFalse(authenticateResult.Succeeded);
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void HandleAuthenticateAsync_IfTheCertificatePrincipalFactoryThrowsAnException_And_IfTheResultFromTheCertificateAuthenticationFailedContextIsNull_ShouldThrowAnInvalidOperationException()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var clientCertificate = new CertificateBuilder().Build())
			{
				var unitTestContext = new UnitTestContextBuilder
				{
					HttpContext =
					{
						Connection =
						{
							ClientCertificate = clientCertificate
						},
						Request =
						{
							IsHttps = true
						}
					}
				}.Build();

				Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
				Assert.IsNotNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);

				var certificatePrincipalFactoryException = new InvalidOperationException("Exception from certificate-principal-factory.");

				var certificatePrincipalFactoryMock = new Mock<ICertificatePrincipalFactory>();
				certificatePrincipalFactoryMock
					.Setup(certificatePrincipalFactory => certificatePrincipalFactory.Create(It.IsAny<string>(), It.IsAny<X509Certificate2>(), It.IsAny<string>()))
					.Throws(certificatePrincipalFactoryException);

				var certificateValidatorMock = new Mock<ICertificateValidator>();
				certificateValidatorMock
					.Setup(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()))
					.Returns(Task.FromResult(this.CreateValidValidationResult()));

				var certificateAuthenticationHandler = this.CreateAuthenticationHandler(certificatePrincipalFactoryMock.Object, certificateValidatorMock.Object, unitTestContext);

				try
				{
					_ = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;
				}
				catch(Exception exception)
				{
					if(exception is AggregateException aggregateException && aggregateException.InnerExceptions.Count == 1)
					{
						if(aggregateException.InnerExceptions.First() is InvalidOperationException invalidOperationException && string.Equals(_authenticationExceptionMessage, invalidOperationException.Message, StringComparison.Ordinal))
						{
							if(ReferenceEquals(certificatePrincipalFactoryException, invalidOperationException.InnerException))
								throw invalidOperationException;
						}
					}
				}
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheCertificateValidatorThrowsAnException_And_IfLoggingIsDisabled_ShouldNotLogAnError()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var clientCertificate = new CertificateBuilder().Build())
			{
				var unitTestContext = new UnitTestContextBuilder
				{
					HttpContext =
					{
						Connection =
						{
							ClientCertificate = clientCertificate
						},
						Request =
						{
							IsHttps = true
						}
					},
					LoggerFactory =
					{
						Enabled = false
					}
				}.Build();

				Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
				Assert.IsNotNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);
				Assert.IsFalse(unitTestContext.LoggerFactory.Enabled);
				Assert.IsFalse(unitTestContext.Logs.Any());

				var certificateValidatorException = new InvalidOperationException("Exception from certificate-validator.");

				var certificateValidatorMock = new Mock<ICertificateValidator>();
				certificateValidatorMock
					.Setup(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()))
					.Throws(certificateValidatorException);

				var certificateAuthenticationHandler = this.CreateAuthenticationHandler(certificateValidatorMock.Object, unitTestContext);

				certificateAuthenticationHandler.Events.OnAuthenticationFailed = async context =>
				{
					context.Fail(context.Exception);

					await Task.CompletedTask.ConfigureAwait(false);
				};

				_ = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;

				var log = unitTestContext.Logs.FirstOrDefault(item => string.Equals(item.Message, _authenticationExceptionMessage, StringComparison.Ordinal));
				Assert.IsNull(log);
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheCertificateValidatorThrowsAnException_And_IfLoggingIsEnabled_ShouldLogAnError()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var clientCertificate = new CertificateBuilder().Build())
			{
				var unitTestContext = new UnitTestContextBuilder
				{
					HttpContext =
					{
						Connection =
						{
							ClientCertificate = clientCertificate
						},
						Request =
						{
							IsHttps = true
						}
					}
				}.Build();

				Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
				Assert.IsNotNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);
				Assert.IsTrue(unitTestContext.LoggerFactory.Enabled);
				Assert.IsFalse(unitTestContext.Logs.Any());

				var certificateValidatorException = new InvalidOperationException("Exception from certificate-validator.");

				var certificateValidatorMock = new Mock<ICertificateValidator>();
				certificateValidatorMock
					.Setup(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()))
					.Throws(certificateValidatorException);

				var certificateAuthenticationHandler = this.CreateAuthenticationHandler(certificateValidatorMock.Object, unitTestContext);

				certificateAuthenticationHandler.Events.OnAuthenticationFailed = async context =>
				{
					context.Fail(context.Exception);

					await Task.CompletedTask.ConfigureAwait(false);
				};

				_ = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;

				var log = unitTestContext.Logs.FirstOrDefault(item => string.Equals(item.Message, _authenticationExceptionMessage, StringComparison.Ordinal));
				Assert.IsNotNull(log);
				Assert.AreEqual(LogLevel.Error, log.LogLevel);
				Assert.IsTrue(ReferenceEquals(certificateValidatorException, log.Exception));
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheCertificateValidatorThrowsAnException_And_IfTheResultFromTheCertificateAuthenticationFailedContextIsNotNull_ShouldReturnTheResultFromTheCertificateAuthenticationFailedContext()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var clientCertificate = new CertificateBuilder().Build())
			{
				var unitTestContext = new UnitTestContextBuilder
				{
					HttpContext =
					{
						Connection =
						{
							ClientCertificate = clientCertificate
						},
						Request =
						{
							IsHttps = true
						}
					}
				}.Build();

				Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
				Assert.IsNotNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);

				var certificateValidatorException = new InvalidOperationException("Exception from certificate-validator.");

				var certificateValidatorMock = new Mock<ICertificateValidator>();
				certificateValidatorMock
					.Setup(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()))
					.Throws(certificateValidatorException);

				var certificateAuthenticationHandler = this.CreateAuthenticationHandler(certificateValidatorMock.Object, unitTestContext);

				certificateAuthenticationHandler.Events.OnAuthenticationFailed = async context =>
				{
					context.Fail(context.Exception);

					await Task.CompletedTask.ConfigureAwait(false);
				};

				var authenticateResult = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;

				Assert.IsNotNull(authenticateResult);
				Assert.IsNotNull(authenticateResult.Failure);
				Assert.AreEqual(_authenticationExceptionMessage, authenticateResult.Failure.Message);
				Assert.AreEqual(certificateValidatorException, authenticateResult.Failure.InnerException);
				Assert.IsFalse(authenticateResult.None);
				Assert.IsFalse(authenticateResult.Succeeded);
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void HandleAuthenticateAsync_IfTheCertificateValidatorThrowsAnException_And_IfTheResultFromTheCertificateAuthenticationFailedContextIsNull_ShouldThrowAnInvalidOperationException()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var clientCertificate = new CertificateBuilder().Build())
			{
				var unitTestContext = new UnitTestContextBuilder
				{
					HttpContext =
					{
						Connection =
						{
							ClientCertificate = clientCertificate
						},
						Request =
						{
							IsHttps = true
						}
					}
				}.Build();

				Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
				Assert.IsNotNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);

				var certificateValidatorException = new InvalidOperationException("Exception from certificate-validator.");

				var certificateValidatorMock = new Mock<ICertificateValidator>();
				certificateValidatorMock
					.Setup(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()))
					.Throws(certificateValidatorException);

				var certificateAuthenticationHandler = this.CreateAuthenticationHandler(certificateValidatorMock.Object, unitTestContext);

				try
				{
					_ = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;
				}
				catch(Exception exception)
				{
					if(exception is AggregateException aggregateException && aggregateException.InnerExceptions.Count == 1)
					{
						if(aggregateException.InnerExceptions.First() is InvalidOperationException invalidOperationException && string.Equals(_authenticationExceptionMessage, invalidOperationException.Message, StringComparison.Ordinal))
						{
							if(ReferenceEquals(certificateValidatorException, invalidOperationException.InnerException))
								throw invalidOperationException;
						}
					}
				}
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheClientCertificateIsNotValid_And_IfLoggingIsDisabled_ShouldNotLogAWarning()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var clientCertificate = new CertificateBuilder().Build())
			{
				var unitTestContext = new UnitTestContextBuilder
				{
					HttpContext =
					{
						Connection =
						{
							ClientCertificate = clientCertificate
						},
						Request =
						{
							IsHttps = true
						}
					},
					LoggerFactory =
					{
						Enabled = false
					}
				}.Build();

				Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
				Assert.IsNotNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);
				Assert.IsFalse(unitTestContext.LoggerFactory.Enabled);
				Assert.IsFalse(unitTestContext.Logs.Any());

				var certificateValidatorMock = new Mock<ICertificateValidator>();
				certificateValidatorMock
					.Setup(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()))
					.Returns(Task.FromResult(this.CreateInvalidValidationResult()));

				var certificateAuthenticationHandler = this.CreateAuthenticationHandler(certificateValidatorMock.Object, unitTestContext);

				_ = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;

				var log = unitTestContext.Logs.FirstOrDefault(item => string.Equals(item.Message, _invalidClientCertificateMessage, StringComparison.Ordinal));
				Assert.IsNull(log);
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheClientCertificateIsNotValid_And_IfLoggingIsEnabled_ShouldLogAWarning()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var clientCertificate = new CertificateBuilder().Build())
			{
				var unitTestContext = new UnitTestContextBuilder
				{
					HttpContext =
					{
						Connection =
						{
							ClientCertificate = clientCertificate
						},
						Request =
						{
							IsHttps = true
						}
					}
				}.Build();

				Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
				Assert.IsNotNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);
				Assert.IsTrue(unitTestContext.LoggerFactory.Enabled);
				Assert.IsFalse(unitTestContext.Logs.Any());

				var certificateValidatorMock = new Mock<ICertificateValidator>();
				certificateValidatorMock
					.Setup(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()))
					.Returns(Task.FromResult(this.CreateInvalidValidationResult()));

				var certificateAuthenticationHandler = this.CreateAuthenticationHandler(certificateValidatorMock.Object, unitTestContext);

				_ = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;

				var log = unitTestContext.Logs.FirstOrDefault(item => string.Equals(item.Message, _invalidClientCertificateMessage, StringComparison.Ordinal));
				Assert.IsNotNull(log);
				Assert.AreEqual(LogLevel.Warning, log.LogLevel);
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheClientCertificateIsNotValid_ShouldReturnAnAuthenticateResultWithFailure()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var clientCertificate = new CertificateBuilder().Build())
			{
				var unitTestContext = new UnitTestContextBuilder
				{
					HttpContext =
					{
						Connection =
						{
							ClientCertificate = clientCertificate
						},
						Request =
						{
							IsHttps = true
						}
					}
				}.Build();

				Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
				Assert.IsNotNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);

				var certificateValidatorMock = new Mock<ICertificateValidator>();
				certificateValidatorMock
					.Setup(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()))
					.Returns(Task.FromResult(this.CreateInvalidValidationResult()));

				var certificateAuthenticationHandler = this.CreateAuthenticationHandler(certificateValidatorMock.Object, unitTestContext);

				var authenticateResult = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;

				Assert.IsNotNull(authenticateResult);
				Assert.IsNotNull(authenticateResult.Failure);
				Assert.AreEqual(_invalidClientCertificateMessage, authenticateResult.Failure.Message);
				Assert.IsFalse(authenticateResult.None);
				Assert.IsFalse(authenticateResult.Succeeded);
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheRequestContainsAClientCertificate_ShouldCallTheCertificateValidator()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var clientCertificate = new CertificateBuilder().Build())
			{
				var unitTestContext = new UnitTestContextBuilder
				{
					HttpContext =
					{
						Connection =
						{
							ClientCertificate = clientCertificate
						},
						Request =
						{
							IsHttps = true
						}
					}
				}.Build();

				Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
				Assert.IsNotNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);

				var certificatePrincipalFactoryMock = new Mock<ICertificatePrincipalFactory>();
				certificatePrincipalFactoryMock
					.Setup(certificatePrincipalFactory => certificatePrincipalFactory.Create(It.IsAny<string>(), It.IsAny<X509Certificate2>(), It.IsAny<string>()))
					.Returns(new ClaimsPrincipal(new ClaimsIdentity()));

				var certificateValidatorMock = new Mock<ICertificateValidator>();
				certificateValidatorMock
					.Setup(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()))
					.Returns(Task.FromResult(this.CreateValidValidationResult()));

				var certificateAuthenticationHandler = this.CreateAuthenticationHandler(certificatePrincipalFactoryMock.Object, certificateValidatorMock.Object, unitTestContext);

				certificateValidatorMock.Verify(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()), Times.Never);

				_ = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;

				certificateValidatorMock.Verify(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()), Times.Once);
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		[SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code")]
		public void HandleAuthenticateAsync_IfTheRequestContainsAClientCertificate_ShouldCallTheCertificateValidatorWithTheCertificateAsParameter()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var clientCertificate = new CertificateBuilder().Build())
			{
				var unitTestContext = new UnitTestContextBuilder
				{
					HttpContext =
					{
						Connection =
						{
							ClientCertificate = clientCertificate
						},
						Request =
						{
							IsHttps = true
						}
					}
				}.Build();

				Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
				Assert.IsNotNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);

				var certificatePrincipalFactoryMock = new Mock<ICertificatePrincipalFactory>();
				certificatePrincipalFactoryMock
					.Setup(certificatePrincipalFactory => certificatePrincipalFactory.Create(It.IsAny<string>(), It.IsAny<X509Certificate2>(), It.IsAny<string>()))
					.Returns(new ClaimsPrincipal(new ClaimsIdentity()));

				X509Certificate2 certificateParameter = null;

				var certificateValidatorMock = new Mock<ICertificateValidator>();
				certificateValidatorMock
					.Setup(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()))
					.Callback<X509Certificate2, CertificateValidatorOptions>((certificate, options) => { certificateParameter = certificate; })
					.Returns(Task.FromResult(this.CreateValidValidationResult()));

				var certificateAuthenticationHandler = this.CreateAuthenticationHandler(certificatePrincipalFactoryMock.Object, certificateValidatorMock.Object, unitTestContext);

				Assert.IsNull(certificateParameter);

				_ = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;

				Assert.IsTrue(ReferenceEquals(clientCertificate, certificateParameter));
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheRequestContainsAClientCertificate_ShouldCallTheCertificateValidatorWithTheOptionsAsParameter()
		{
			// ReSharper disable ConvertToUsingDeclaration
			using(var clientCertificate = new CertificateBuilder().Build())
			{
				var unitTestContext = new UnitTestContextBuilder
				{
					HttpContext =
					{
						Connection =
						{
							ClientCertificate = clientCertificate
						},
						Request =
						{
							IsHttps = true
						}
					}
				}.Build();

				Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
				Assert.IsNotNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);

				var certificatePrincipalFactoryMock = new Mock<ICertificatePrincipalFactory>();
				certificatePrincipalFactoryMock
					.Setup(certificatePrincipalFactory => certificatePrincipalFactory.Create(It.IsAny<string>(), It.IsAny<X509Certificate2>(), It.IsAny<string>()))
					.Returns(new ClaimsPrincipal(new ClaimsIdentity()));

				CertificateValidatorOptions optionsParameter = null;

				var certificateValidatorMock = new Mock<ICertificateValidator>();
				certificateValidatorMock
					.Setup(certificateValidator => certificateValidator.ValidateAsync(It.IsAny<X509Certificate2>(), It.IsAny<CertificateValidatorOptions>()))
					.Callback<X509Certificate2, CertificateValidatorOptions>((certificate, options) => { optionsParameter = options; })
					.Returns(Task.FromResult(this.CreateValidValidationResult()));

				var certificateAuthenticationHandler = this.CreateAuthenticationHandler(certificatePrincipalFactoryMock.Object, certificateValidatorMock.Object, unitTestContext);

				Assert.IsNull(optionsParameter);

				_ = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;

				Assert.IsTrue(ReferenceEquals(certificateAuthenticationHandler.Options.Validator, optionsParameter));
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheRequestDoesNotContainAClientCertificate_And_IfLoggingIsDisabled_ShouldNotLogADebug()
		{
			var unitTestContext = new UnitTestContextBuilder
			{
				HttpContext =
				{
					Request =
					{
						IsHttps = true
					}
				},
				LoggerFactory =
				{
					Enabled = false
				}
			}.Build();

			Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
			Assert.IsNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);
			Assert.IsFalse(unitTestContext.LoggerFactory.Enabled);
			Assert.IsFalse(unitTestContext.Logs.Any());

			_ = this.CreateAuthenticationHandler(unitTestContext).HandleAuthenticateAsync().Result;

			var log = unitTestContext.Logs.FirstOrDefault(item => string.Equals(item.Message, _noClientCertificateLogMessage, StringComparison.Ordinal));
			Assert.IsNull(log);
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheRequestDoesNotContainAClientCertificate_And_IfLoggingIsEnabled_ShouldLogADebug()
		{
			var unitTestContext = new UnitTestContextBuilder
			{
				HttpContext =
				{
					Request =
					{
						IsHttps = true
					}
				}
			}.Build();

			Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
			Assert.IsNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);
			Assert.IsTrue(unitTestContext.LoggerFactory.Enabled);
			Assert.IsFalse(unitTestContext.Logs.Any());

			_ = this.CreateAuthenticationHandler(unitTestContext).HandleAuthenticateAsync().Result;

			var log = unitTestContext.Logs.FirstOrDefault(item => string.Equals(item.Message, _noClientCertificateLogMessage, StringComparison.Ordinal));
			Assert.IsNotNull(log);
			Assert.AreEqual(LogLevel.Debug, log.LogLevel);
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheRequestDoesNotContainAClientCertificate_ShouldReturnAnAuthenticateResultWhereNoneIsTrue()
		{
			var unitTestContext = new UnitTestContextBuilder
			{
				HttpContext =
				{
					Request =
					{
						IsHttps = true
					}
				}
			}.Build();

			Assert.IsTrue(unitTestContext.HttpContext.Request.IsHttps);
			Assert.IsNull(unitTestContext.HttpContext.Connection.GetClientCertificateAsync().Result);

			var certificateAuthenticationHandler = this.CreateAuthenticationHandler(unitTestContext);

			var authenticateResult = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;

			Assert.IsNotNull(authenticateResult);
			Assert.IsTrue(authenticateResult.None);
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheRequestIsNotHttps_And_IfLoggingIsDisabled_ShouldNotLogADebug()
		{
			var unitTestContext = new UnitTestContextBuilder
			{
				LoggerFactory =
				{
					Enabled = false
				}
			}.Build();

			Assert.IsFalse(unitTestContext.HttpContext.Request.IsHttps);
			Assert.IsFalse(unitTestContext.LoggerFactory.Enabled);
			Assert.IsFalse(unitTestContext.Logs.Any());

			_ = this.CreateAuthenticationHandler(unitTestContext).HandleAuthenticateAsync().Result;

			var log = unitTestContext.Logs.FirstOrDefault(item => string.Equals(item.Message, _noHttpsLogMessage, StringComparison.Ordinal));
			Assert.IsNull(log);
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheRequestIsNotHttps_And_IfLoggingIsEnabled_ShouldLogADebug()
		{
			var unitTestContext = new UnitTestContextBuilder().Build();

			Assert.IsFalse(unitTestContext.HttpContext.Request.IsHttps);
			Assert.IsTrue(unitTestContext.LoggerFactory.Enabled);
			Assert.IsFalse(unitTestContext.Logs.Any());

			_ = this.CreateAuthenticationHandler(unitTestContext).HandleAuthenticateAsync().Result;

			var log = unitTestContext.Logs.FirstOrDefault(item => string.Equals(item.Message, _noHttpsLogMessage, StringComparison.Ordinal));
			Assert.IsNotNull(log);
			Assert.AreEqual(LogLevel.Debug, log.LogLevel);
		}

		[TestMethod]
		public void HandleAuthenticateAsync_IfTheRequestIsNotHttps_ShouldReturnAnAuthenticateResultWhereNoneIsTrue()
		{
			var unitTestContext = new UnitTestContextBuilder().Build();

			Assert.IsFalse(unitTestContext.HttpContext.Request.IsHttps);

			var certificateAuthenticationHandler = this.CreateAuthenticationHandler(unitTestContext);

			var authenticateResult = certificateAuthenticationHandler.HandleAuthenticateAsync().Result;

			Assert.IsNotNull(authenticateResult);
			Assert.IsTrue(authenticateResult.None);
		}

		#endregion
	}
}