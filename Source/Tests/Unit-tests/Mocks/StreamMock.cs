using System.IO;
using System.Text;

namespace UnitTests.Mocks
{
	public class StreamMock : MemoryStream
	{
		#region Properties

		public virtual string Value => Encoding.UTF8.GetString(this.ToArray());

		#endregion
	}
}