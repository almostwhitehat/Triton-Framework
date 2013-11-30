using System.Collections;
using System.Collections.Specialized;

namespace Triton.Controller.Request
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>MvcRequest</b> is the interface to be implemented for any type of request
	/// handled by the <b>Controller</b>.
	/// </summary>
	///	<author>Scott Dyke</author>
	public interface MvcRequest
	{
		/// <summary>
		/// Indexer for a <b>MvcRequest</b>.  Gets or Sets the parameter 
		/// with the given name.
		/// </summary>
		string this[string paramName] { get; set; }


		/// <summary>
		/// Gets a collection of the parameters of the request.
		/// </summary>
		NameValueCollection Params { get; }


		/// <summary>
		/// Gets a collection of objects used internally for the processing of the request.
		/// </summary>
		IDictionary Items { get; }


		/// <summary>
		/// Gets the version of the target "page" for rendering the response.
		/// </summary>
		string Version { get; set; }


		/// <summary>
		/// Gets the IP address of the origin of the request.
		/// </summary>
		string IP { get; }

		/// <summary>
		/// Gets the logon user of the client request.
		/// </summary>
		string UserName { get; }


		/// <summary>
		/// Gets the collection of client-uploaded files.
		/// </summary>
		MvcPostedFileCollection Files { get; }


		///<summary>
		/// Retrieves the specific Type of object from the Items Dictionary.
		///</summary>
		///<param name="key">Object key.</param>
		///<typeparam name="T">Type of object to return.</typeparam>
		///<returns>The corresponding key value.</returns>
		T GetItem<T>(object key);


		/// <summary>
		/// Returns a <b>MvcCookie</b> from the request with the given name, 
		/// or null if no cookie with that name exists.
		/// </summary>
		/// <param name="name">The name of the cookie to get.</param>
		/// <returns>A <b>MvcCookie</b> with the given name, or null if no 
		///		cookie with that name exists.</returns>
		IMvcCookie GetCookie(string name);


		/// <summary>
		/// Returns a <b>MvcCookie</b> from the response with the given name, 
		/// or null if no cookie with that name exists.
		/// </summary>
		/// <param name="name">The name of the response cookie to get.</param>
		/// <returns>A <b>MvcCookie</b> with the given name, or null if no 
		///		cookie with that name exists in the response.</returns>
		IMvcCookie GetResponseCookie(string name);


		/// <summary>
		/// Sets a <b>MvcCookie</b> in the response.  If the cookie already
		/// exists, its information is updated, if not, it is created.
		/// </summary>
		/// <param name="cookie">The <b>MvcCookie</b> to set in the response.</param>
		void SetResponseCookie(IMvcCookie cookie);


		/// <summary>
		/// Writes the given content to the request's response.
		/// </summary>
		/// <param name="content">The content to write to the response.</param>
		/// <param name="endResponse"><b>True</b> if the response is to be ended,
		///		<b>false</b> if not.</param>
		void WriteResponse(
			string content,
			bool endResponse);


		/// <summary>
		/// Executes the given URL and returns the content returned by the URL.
		/// </summary>
		/// <param name="url">The URL to execute.</param>
		/// <returns>The content returned from the given URL.</returns>
		string Execute(string url);


		/// <summary>
		/// Transfers processing of the request to the given URL.
		/// </summary>
		/// <param name="url">The URL to transfer processing to.</param>
		void Transfer(string url);


		/// <summary>
		/// Redirects a client to a new URL and specifies the new URL.
		/// </summary>
		/// <param name="url">The target location.</param>
		void Redirect(string url);


		/// <summary>
		/// Validates data submitted by a client and raises an exception 
		/// if potentially dangerous data is present.
		/// </summary>
		void ValidateInput();
	}
}