using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Logging.Extensions;
using RegionOrebroLan.Security.Cryptography.Validation;
using RegionOrebroLan.Web.Authentication.Certificate.Claims;
using RegionOrebroLan.Web.Authentication.Certificate.Events;

namespace RegionOrebroLan.Web.Authentication.Certificate
{
	[CLSCompliant(false)]
	public class CertificateAuthenticationHandler : AuthenticationHandler<CertificateAuthenticationOptions>
	{
		#region Constructors

		public CertificateAuthenticationHandler(ICertificatePrincipalFactory certificatePrincipalFactory, ICertificateValidator certificateValidator, ILoggerFactory loggerFactory, IOptionsMonitor<CertificateAuthenticationOptions> options, ISystemClock systemClock, UrlEncoder urlEncoder) : base(options, loggerFactory, urlEncoder, systemClock)
		{
			this.CertificatePrincipalFactory = certificatePrincipalFactory ?? throw new ArgumentNullException(nameof(certificatePrincipalFactory));
			this.CertificateValidator = certificateValidator ?? throw new ArgumentNullException(nameof(certificateValidator));
		}

		#endregion

		#region Properties

		protected internal virtual ICertificatePrincipalFactory CertificatePrincipalFactory { get; }
		protected internal virtual ICertificateValidator CertificateValidator { get; }

		protected internal new virtual CertificateAuthenticationEvents Events
		{
			get => (CertificateAuthenticationEvents) base.Events;
			set => base.Events = value;
		}

		#endregion

		#region Methods

		protected override async Task<object> CreateEventsAsync()
		{
			return await Task.FromResult<object>(new CertificateAuthenticationEvents()).ConfigureAwait(false);
		}

		protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			try
			{
				if(!this.Context.Request.IsHttps)
				{
					this.Logger.LogDebugIfEnabled("The request is not https. Client-certificates are only available over https.");
					return AuthenticateResult.NoResult();
				}

				var clientCertificate = await Context.Connection.GetClientCertificateAsync().ConfigureAwait(false);

				if(clientCertificate == null)
				{
					this.Logger.LogDebugIfEnabled("The request does not contain a client-certificate.");
					return AuthenticateResult.NoResult();
				}

				var validationResult = await this.CertificateValidator.ValidateAsync(clientCertificate, this.Options.Validator).ConfigureAwait(false);

				if(!validationResult.Valid)
				{
					var separator = $"{Environment.NewLine} - ";
					var message = $"Client-certificate \"{clientCertificate.Subject}\" failed validation. Exceptions:{separator}{string.Join(separator, validationResult.Exceptions)}";

					this.Logger.LogWarningIfEnabled(message);

					return AuthenticateResult.Fail(message);
				}

				var certificatePrincipal = this.CertificatePrincipalFactory.Create(this.Scheme.Name, clientCertificate, this.Options.ClaimsIssuer);

				var certificateValidatedContext = new CertificateValidatedContext(this.Context, this.Options, this.Scheme)
				{
					ClientCertificate = clientCertificate,
					Principal = certificatePrincipal
				};

				await this.Events.CertificateValidated(certificateValidatedContext).ConfigureAwait(false);

				if(certificateValidatedContext.Result != null)
					return certificateValidatedContext.Result;

				certificateValidatedContext.Success();
				return certificateValidatedContext.Result;
			}
			catch(Exception exception)
			{
				const string message = "Could not handle authentication.";

				this.Logger.LogErrorIfEnabled(exception, message);

				var authenticationException = new InvalidOperationException(message, exception);

				var authenticationFailedContext = new CertificateAuthenticationFailedContext(this.Context, this.Options, this.Scheme)
				{
					Exception = authenticationException
				};

				await this.Events.AuthenticationFailed(authenticationFailedContext).ConfigureAwait(false);

				if(authenticationFailedContext.Result != null)
					return authenticationFailedContext.Result;

				throw authenticationException;
			}
		}

		#endregion
	}
}