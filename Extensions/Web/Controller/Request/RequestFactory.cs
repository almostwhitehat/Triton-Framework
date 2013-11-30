using System.IO;
using System.Text;
using System.Web;

namespace Triton.Controller.Request
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>RequestFactory</b> is the factory class for constructing requests that
	/// implement the MvcRequest interface.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class RequestFactory
	{
		/// <summary>
		/// Constructs a MvcRequest of the appropriate type based on the information
		/// provided in the given HttpContext.
		/// </summary>
		/// <param name="httpContext">An <b>HttpContext</b> to construct a MvcRequest for.</param>
		/// <returns>A <b>MvcRequest</b> for the given context.</returns>
		public static MvcRequest Make(
			HttpContext httpContext)
		{
			byte[] requestBytes = new byte[8];

			// Read first 8 character of the request to determine if it's an xml request
			// indicated by: first 8 characters = "<request", and reset the input stream 
			// position to the beginning
			// NOTE: we read directly from the request input stream rather than using a StreamReader
			// because that will close the underlying stream for further operations after calling close or garbage collection
			httpContext.Request.InputStream.Read(requestBytes, 0, 8);
			httpContext.Request.InputStream.Seek(0, SeekOrigin.Begin);

			if ("<request".Equals(Encoding.ASCII.GetString(requestBytes))) {
				return new MvcXmlRequest(httpContext);
			} else {
				return new MvcHttpRequest(httpContext);
			}
		}
	}
}