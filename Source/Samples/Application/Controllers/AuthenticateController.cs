using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Business.Security.Claims;
using Application.Business.Security.Claims.Extensions;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Controllers
{
	public class AuthenticateController : Controller
	{
		#region Fields

		private static readonly IDictionary<string, Tuple<string, string>> _uniqueIdentifierMap = new Dictionary<string, Tuple<string, string>>(StringComparer.OrdinalIgnoreCase);

		#endregion

		#region Constructors

		public AuthenticateController(IOptions<AuthenticationOptions> authenticationOptions, IAuthenticationSchemeProvider authenticationSchemeProvider, ILoggerFactory loggerFactory)
		{
			this.AuthenticationOptions = authenticationOptions ?? throw new ArgumentNullException(nameof(authenticationOptions));
			this.AuthenticationSchemeProvider = authenticationSchemeProvider ?? throw new ArgumentNullException(nameof(authenticationSchemeProvider));

			if(loggerFactory == null)
				throw new ArgumentNullException(nameof(loggerFactory));

			this.Logger = loggerFactory.CreateLogger(this.GetType());
		}

		#endregion

		#region Properties

		protected internal virtual IOptions<AuthenticationOptions> AuthenticationOptions { get; }
		protected internal virtual IAuthenticationSchemeProvider AuthenticationSchemeProvider { get; }
		protected internal virtual ILogger Logger { get; }
		protected internal virtual IDictionary<string, Tuple<string, string>> UniqueIdentifierMap => _uniqueIdentifierMap;

		#endregion

		#region Methods

		public virtual async Task<IActionResult> Callback()
		{
			var authenticateResult = await this.HttpContext.AuthenticateAsync(this.AuthenticationOptions.Value.DefaultSignInScheme);

			if(!authenticateResult.Succeeded)
				throw new InvalidOperationException("Authentication error.");

			var returnUrl = this.ResolveAndValidateReturnUrl(authenticateResult.Properties.Items["returnUrl"]);

			var authenticationScheme = authenticateResult.Properties.Items["scheme"];

			var claims = new HashSet<Claim>(authenticateResult.Principal.Claims, ClaimComparer.Default);
			this.ResolveClaims(authenticationScheme, claims);

			var authenticationProperties = new AuthenticationProperties();
			this.ResolveAuthenticationProperties(authenticateResult, authenticationProperties);

			var claimsIdentity = new ClaimsIdentity(authenticationScheme);
			claimsIdentity.AddClaims(claims);

			await this.HttpContext.SignInAsync(this.AuthenticationOptions.Value.DefaultScheme, new ClaimsPrincipal(claimsIdentity), authenticationProperties);

			await this.HttpContext.SignOutAsync(this.AuthenticationOptions.Value.DefaultSignInScheme);

			return this.Redirect(returnUrl);
		}

		public virtual async Task<IActionResult> Certificate(string authenticationScheme, string returnUrl)
		{
			returnUrl = this.ResolveAndValidateReturnUrl(returnUrl);

			var authenticateResult = await this.HttpContext.AuthenticateAsync(authenticationScheme);

			if(!authenticateResult.Succeeded)
				throw new InvalidOperationException("Authentication error.", authenticateResult.Failure);

			var authenticationProperties = this.CreateAuthenticationProperties(returnUrl, authenticationScheme);

			await this.HttpContext.SignInAsync(this.AuthenticationOptions.Value.DefaultSignInScheme, authenticateResult.Principal, authenticationProperties);

			return this.Redirect(authenticationProperties.RedirectUri);
		}

		protected internal virtual AuthenticationProperties CreateAuthenticationProperties(string returnUrl, string scheme)
		{
			var authenticationProperties = new AuthenticationProperties
			{
				RedirectUri = this.Url.Action(nameof(Callback))
			};

			// This is mainly for ActiveLogin-handlers
			authenticationProperties.SetString("cancelReturnUrl", this.Url.Action("SignIn", "Account", new {returnUrl}));

			authenticationProperties.SetString(nameof(returnUrl), returnUrl);
			authenticationProperties.SetString(nameof(scheme), scheme);

			return authenticationProperties;
		}

		protected internal virtual string GetOrCreateUniqueIdentifier(string authenticationScheme, string remoteUniqueIdentifier)
		{
			foreach(var (key, (provider, identifier)) in this.UniqueIdentifierMap)
			{
				if(string.Equals(provider, authenticationScheme, StringComparison.OrdinalIgnoreCase) && string.Equals(identifier, remoteUniqueIdentifier, StringComparison.OrdinalIgnoreCase))
					return key;
			}

			var uniqueIdentifier = Guid.NewGuid().ToString();

			this.UniqueIdentifierMap.Add(uniqueIdentifier, new Tuple<string, string>(authenticationScheme, remoteUniqueIdentifier));

			return uniqueIdentifier;
		}

		public virtual async Task<IActionResult> Remote(string authenticationScheme, string returnUrl)
		{
			returnUrl = this.ResolveAndValidateReturnUrl(returnUrl);

			return await Task.FromResult(this.Challenge(this.CreateAuthenticationProperties(returnUrl, authenticationScheme), authenticationScheme));
		}

		protected internal virtual string ResolveAndValidateReturnUrl(string returnUrl)
		{
			if(string.IsNullOrEmpty(returnUrl))
				returnUrl = "~/";

			if(!this.Url.IsLocalUrl(returnUrl))
				throw new InvalidOperationException($"\"{returnUrl}\" is an invalid return-url.");

			return returnUrl;
		}

		protected internal virtual void ResolveAuthenticationProperties(AuthenticateResult authenticateResult, AuthenticationProperties authenticationProperties)
		{
			if(authenticateResult == null)
				throw new ArgumentNullException(nameof(authenticateResult));

			const string idTokenName = "id_token";

			var idToken = authenticateResult.Properties.GetTokenValue(idTokenName);

			if(idToken != null)
				authenticationProperties.StoreTokens(new[] {new AuthenticationToken {Name = idTokenName, Value = idToken}});
		}

		protected internal virtual void ResolveClaims(string authenticationScheme, ISet<Claim> claims)
		{
			if(authenticationScheme == null)
				throw new ArgumentNullException(nameof(authenticationScheme));

			if(claims == null)
				throw new ArgumentNullException(nameof(claims));

			var uniqueIdentifierClaim = claims.FindUniqueIdentifierClaim();

			if(uniqueIdentifierClaim == null)
				throw new InvalidOperationException("There is no unique-identifier-claim.");

			var uniqueIdentifier = this.GetOrCreateUniqueIdentifier(authenticationScheme, uniqueIdentifierClaim.Value);

			claims.Remove(uniqueIdentifierClaim);

			claims.Add(new Claim(uniqueIdentifierClaim.Type, uniqueIdentifier));

			var identityProviderClaim = claims.FindIdentityProviderClaim();

			if(identityProviderClaim != null)
				claims.Remove(identityProviderClaim);

			claims.Add(new Claim(ExtendedClaimTypes.IdentityProvider, authenticationScheme));
		}

		#endregion
	}
}