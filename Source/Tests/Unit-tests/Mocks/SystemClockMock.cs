using System;
using Microsoft.AspNetCore.Authentication;

namespace UnitTests.Mocks
{
	public class SystemClockMock : ISystemClock
	{
		#region Properties

		public virtual DateTimeOffset UtcNow { get; set; } = new SystemClock().UtcNow;

		#endregion
	}
}