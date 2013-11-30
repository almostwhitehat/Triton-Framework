using System.Collections;
using System.Collections.Specialized;
using System.Net;

namespace Triton.Controller.Request
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>MvcCoreRequest</b> implements the <b>MvcRequest</b> interface for
	/// non-web related applications.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class MvcCoreRequest : MvcRequest
	{
		/// <summary>
		/// The IP address of the machine the request originated from.
		/// </summary>
		private readonly string ip;

		/// <summary>
		/// A key/value collection that can be used to organize and share data between the
		/// business layer and presentation layer.
		/// </summary>
		private readonly Hashtable items = new Hashtable();

		/// <summary>
		/// The collection of request parameters.
		/// </summary>
		private readonly NameValueCollection parameters = new NameValueCollection();


		/// <summary>
		/// Default constructor.
		/// </summary>
		public MvcCoreRequest()
		{
			try {
				//  get the host name
				string hostName = Dns.GetHostName();
				//  find host by name
				IPHostEntry ipHostEntry = Dns.GetHostEntry(hostName);
				//  get the IP(s)
				IPAddress ipAddr = ipHostEntry.AddressList[0];
				this.ip = ipAddr.ToString();
			} catch {}
		}

		#region MvcRequest Members

		/// <summary>
		/// Indexer for a <b>MvcRequest</b>.  Gets or Sets the parameter 
		/// with the given name.
		/// </summary>
		public string this[string paramName]
		{
			get { return this.parameters[paramName]; }
			set { this.parameters[paramName] = value; }
		}


		/// <summary>
		/// Gets a collection of the parameters of the request.
		/// </summary>
		public NameValueCollection Params
		{
			get { return this.parameters; }
		}


		/// <summary>
		/// Gets a key/value collection that can be used to organize and share data between the
		/// business layer and presentation layer.
		/// </summary>
		public IDictionary Items
		{
			get { return this.items; }
		}


		///<summary>
		/// Retrieves the specific Type of object from the Items Dictionary.
		///</summary>
		///<param name="key">Object key.</param>
		///<typeparam name="T">Type of object to return.</typeparam>
		///<returns>The corresponding key value.</returns>
		public T GetItem<T>(object key)
		{
			return (T) this.Items[key];
		}


		/// <summary>
		/// Gets the version of the target "page" for rendering the response.
		/// </summary>
		public string Version { get; set; }


		/// <summary>
		/// Gets the IP address of the origin of the request.
		/// </summary>
		public string IP
		{
			get { return this.ip; }
		}


		/// <summary>
		/// Gets the logon user of the client request.
		/// </summary>
		/// <remarks>
		/// Unimplemented for MvcCoreRequest.
		/// </remarks>
		public string UserName
		{
			get { return null; }
		}


		/// <summary>
		/// Gets the collection of client-uploaded files.
		/// </summary>
		/// <remarks>
		/// Unimplemented for MvcCoreRequest.
		/// </remarks>
		public MvcPostedFileCollection Files
		{
			get { return null; }
		}


		/// <summary>
		/// Returns a <b>MvcCookie</b> from the request with the given name, 
		/// or null if no cookie with that name exists.
		/// </summary>
		/// <param name="name">The name of the cookie to get.</param>
		/// <returns>A <b>MvcCookie</b> with the given name, or null if no 
		///		cookie with that name exists.</returns>
		/// <remarks>
		/// Unimplemented for MvcCoreRequest.
		/// </remarks>
		public IMvcCookie GetCookie(
			string name)
		{
			return null;
		}


		/// <summary>
		/// Returns a <b>MvcCookie</b> from the response with the given name, 
		/// or null if no cookie with that name exists.
		/// </summary>
		/// <param name="name">The name of the response cookie to get.</param>
		/// <returns>A <b>MvcCookie</b> with the given name, or null if no 
		///		cookie with that name exists in the response.</returns>
		/// <remarks>
		/// Unimplemented for MvcCoreRequest.
		/// </remarks>
		public IMvcCookie GetResponseCookie(
			string name)
		{
			return null;
		}


		/// <summary>
		/// Sets a <b>MvcCookie</b> in the response.  If the cookie already
		/// exists, its information is updated, if not, it is created.
		/// </summary>
		/// <param name="cookie">The <b>MvcCookie</b> to set in the response.</param>
		/// <remarks>
		/// Unimplemented for MvcCoreRequest.
		/// </remarks>
		public void SetResponseCookie(
			IMvcCookie cookie) {}


		/// <summary>
		/// Writes the given content to the request's response.
		/// </summary>
		/// <param name="content">The content to write to the response.</param>
		/// <param name="endResponse"><b>True</b> if the response is to be ended,
		///		<b>false</b> if not.</param>
		/// <remarks>
		/// Unimplemented for MvcCoreRequest.
		/// </remarks>
		public void WriteResponse(
			string content,
			bool endResponse) {}


		/// <summary>
		/// Executes the given URL and returns the content returned by the URL.
		/// </summary>
		/// <param name="url">The URL to execute.</param>
		/// <returns>The content returned from the given URL.</returns>
		/// <remarks>
		/// Unimplemented for MvcCoreRequest.
		/// </remarks>
		public string Execute(
			string url)
		{
			return null;
		}


		/// <summary>
		/// Transfers processing of the request to the given URL.
		/// </summary>
		/// <param name="url">The URL to transfer processing to.</param>
		/// <remarks>
		/// Unimplemented for MvcCoreRequest.
		/// </remarks>
		public void Transfer(
			string url) {}


		/// <summary>
		/// Redirects a client to a new URL and specifies the new URL.
		/// </summary>
		/// <param name="url">The target location.</param>
		/// <remarks>
		/// Unimplemented for MvcCoreRequest.
		/// </remarks>
		public void Redirect(
			string url) {}


		/// <summary>
		/// Validates data submitted by a client and raises an exception 
		/// if potentially dangerous data is present.
		/// </summary>
		/// <remarks>
		/// Unimplemented for MvcCoreRequest.
		/// </remarks>
		public void ValidateInput() {}

		#endregion
	}
}