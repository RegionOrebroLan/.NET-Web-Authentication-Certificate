using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace Application.Models
{
	public class SignInViewModel
	{
		#region Properties

		public virtual IList<AuthenticationScheme> AuthenticationSchemes { get; } = new List<AuthenticationScheme>();
		public virtual string ReturnUrl { get; set; }

		#endregion
	}
}