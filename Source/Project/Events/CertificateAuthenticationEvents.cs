using System;
using System.Threading.Tasks;

namespace RegionOrebroLan.Web.Authentication.Certificate.Events
{
	[CLSCompliant(false)]
	public class CertificateAuthenticationEvents
	{
		#region Properties

		public virtual Func<CertificateAuthenticationFailedContext, Task> OnAuthenticationFailed { get; set; } = context => Task.CompletedTask;
		public virtual Func<CertificateValidatedContext, Task> OnCertificateValidated { get; set; } = context => Task.CompletedTask;

		#endregion

		#region Methods

		public virtual Task AuthenticationFailed(CertificateAuthenticationFailedContext context)
		{
			return this.OnAuthenticationFailed(context);
		}

		public virtual Task CertificateValidated(CertificateValidatedContext context)
		{
			return this.OnCertificateValidated(context);
		}

		#endregion
	}
}