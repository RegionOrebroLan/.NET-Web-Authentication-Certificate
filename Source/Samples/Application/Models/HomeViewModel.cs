using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace Application.Models
{
	public class HomeViewModel
	{
		#region Properties

		public virtual IList<AuthenticationScheme> AuthenticationSchemes { get; } = new List<AuthenticationScheme>();

		#endregion
	}
}