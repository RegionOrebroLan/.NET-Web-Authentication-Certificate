using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace RegionOrebroLan.Web.Authentication.Certificate.UnitTests.Mocks
{
	public class OptionsMonitorMock<T> : IOptionsMonitor<T> where T : new()
	{
		#region Constructors

		public OptionsMonitorMock(Action<T> optionsConfigurer)
		{
			this.OptionsConfigurer = optionsConfigurer;
		}

		#endregion

		#region Properties

		public virtual T CurrentValue => this.Get(Options.DefaultName);
		protected virtual Action<T> OptionsConfigurer { get; }

		#endregion

		#region Methods

		[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
		public virtual T Get(string name)
		{
			var options = new T();

			this.OptionsConfigurer?.Invoke(options);

			return options;
		}

		public virtual IDisposable OnChange(Action<T, string> listener)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}