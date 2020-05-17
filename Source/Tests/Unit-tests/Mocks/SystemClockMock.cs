using System;
using Microsoft.AspNetCore.Authentication;

namespace RegionOrebroLan.Web.Authentication.Certificate.UnitTests.Mocks
{
	public class SystemClockMock : ISystemClock
	{
		#region Properties

		public virtual DateTimeOffset UtcNow { get; set; } = new SystemClock().UtcNow;

		#endregion
	}
}