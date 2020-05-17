using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace RegionOrebroLan.Web.Authentication.Certificate.IntegrationTests
{
	[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
	public static class Global
	{
		#region Fields

		// ReSharper disable PossibleNullReferenceException
		public static readonly string ProjectDirectoryPath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
		// ReSharper restore PossibleNullReferenceException

		#endregion
	}
}