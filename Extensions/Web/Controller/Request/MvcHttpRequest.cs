using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using Triton.Controller.Utilities;

namespace Triton.Controller.Request
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>MvcHttpRequest</b> is an <i>adaptor</i> class that adapts a .net request
	/// (represented by a HttpContext) to the MvcRequest interface.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class MvcHttpRequest : MvcRequest
	{
		/// <summary>
		/// The underlying .net request HttpContext this 
		/// </summary>
		private readonly HttpContext httpContext;

		private readonly string ip;
		private readonly string userName;

		/// <summary>
		/// The local collection of parameters.
		/// </summary>
		private NameValueCollection localParams;

		/// <summary>
		/// The collection of client uploaded files.
		/// </summary>
		private MvcPostedFileCollection postedFiles;

		/// <summary>
		/// Flag to indicate whether or not we should use the local collection,
		/// or the underlying HttpContext collection.
		/// </summary>
		private bool useLocal;


		/// <summary>
		/// Constructs a new MvcHttpRequest from the given HttpContext.
		/// </summary>
		/// <param name="httpContext"></param>
		public MvcHttpRequest(
			HttpContext httpContext)
		{
			this.httpContext = httpContext;
			this.ip = WebUtilities.GetIP(httpContext.Request);
			this.userName = WebUtilities.GetUserName(httpContext.Request);

			//  if there are posted files included in the HttpRequest,
			//  build the internal collection and add the files
			if ((httpContext.Request.Files != null) && (httpContext.Request.Files.Count > 0)) {
				this.postedFiles = new MvcPostedFileCollection();
				foreach (string fieldName in httpContext.Request.Files) {
					HttpPostedFile file = httpContext.Request.Files[fieldName];
					if (file.ContentLength > 0) {
						this.postedFiles.Add(fieldName, new MvcHttpPostedFile(file));
					}
				}
			}
		}

		#region MvcRequest members

		/// <summary>
		/// Indexer for a <b>MvcRequest</b>.  Gets or Sets the parameter 
		/// with the given name.
		/// </summary>
		public string this[string paramName]
		{
			get
			{
				string param = this.useLocal ? this.localParams[paramName] : this.httpContext.Request[paramName];

				return param;
			}
			set { this.LocalParams[paramName] = value; }
		}


		/// <summary>
		/// Gets a collection of the parameters of the request.
		/// </summary>
		public NameValueCollection Params
		{
			get { return this.LocalParams; }
		}


		/// <summary>
		/// Gets a collection of objects used internally for the processing of the request.
		/// </summary>
		public IDictionary Items
		{
			get { return this.httpContext.Items; }
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
		public string UserName
		{
			get { return this.userName; }
		}


		/// <summary>
		/// Gets the collection of client-uploaded files (Multipart MIME format).
		/// </summary>
		public MvcPostedFileCollection Files
		{
			get
			{
				//  make sure we never return "null" to avoid null reference exceptions
				if (this.postedFiles == null) {
					this.postedFiles = new MvcPostedFileCollection();
				}

				return this.postedFiles;
			}
		}


		/// <summary>
		/// Returns a <b>MvcCookie</b> from the request with the given name, 
		/// or null if no cookie with that name exists.
		/// </summary>
		/// <param name="name">The name of the cookie to get.</param>
		/// <returns>A <b>MvcCookie</b> with the given name, or null if no 
		///		cookie with that name exists.</returns>
		public IMvcCookie GetCookie(
			string name)
		{
			HttpCookie cookie = this.httpContext.Request.Cookies[name];

			return ((cookie == null) ? null : new MvcHttpCookie(cookie));
		}


		/// <summary>
		/// Returns a <b>MvcCookie</b> from the response with the given name, 
		/// or null if no cookie with that name exists.
		/// </summary>
		/// <param name="name">The name of the response cookie to get.</param>
		/// <returns>A <b>MvcCookie</b> with the given name, or null if no 
		///		cookie with that name exists in the response.</returns>
		public IMvcCookie GetResponseCookie(
			string name)
		{
			HttpCookie cookie = this.httpContext.Response.Cookies[name];

			return ((cookie == null) ? null : new MvcHttpCookie(cookie));
		}


		/// <summary>
		/// Sets a <b>MvcCookie</b> in the response.  If the cookie already
		/// exists, its information is updated, if not, it is created.
		/// </summary>
		/// <param name="cookie">The <b>MvcCookie</b> to set in the response.</param>
		public void SetResponseCookie(
			IMvcCookie cookie)
		{
			if (cookie != null) {
				//  see if there is already a response cookie with this name
				HttpCookie httpCookie = this.httpContext.Response.Cookies[cookie.Name];

				//  if there is no existing cookie, add it
				if (httpCookie == null) {
					this.httpContext.Response.Cookies.Add(((MvcHttpCookie)cookie).HttpCookie);

					//  if the cookie already exists, update its values
				} else {
					httpCookie.Domain = cookie.Domain;
					httpCookie.Expires = cookie.Expires;
					httpCookie.Path = cookie.Path;
					httpCookie.Secure = cookie.Secure;
					httpCookie.Value = cookie.Value;
				}
			}
		}


		/// <summary>
		/// Writes the given content to the request's response.
		/// </summary>
		/// <param name="content">The content to write to the response.</param>
		/// <param name="endResponse"><b>True</b> if the response is to be ended,
		///		<b>false</b> if not.</param>
		public void WriteResponse(
			string content,
			bool endResponse)
		{
			this.httpContext.Response.Write(content);

			if (endResponse) {
				this.httpContext.Response.End();
			}
		}


		/// <summary>
		/// Executes the given URL and returns the content returned by the URL.
		/// </summary>
		/// <param name="url">The URL to execute.</param>
		/// <returns>The content returned from the given URL.</returns>
		public string Execute(
			string url)
		{
			StringWriter strWriter = new StringWriter();

			try {
				this.httpContext.Server.Execute(url, strWriter);
			} finally {
				strWriter.Close();
			}

			//  get the content of the executed url
			string content = strWriter.GetStringBuilder().ToString();

			return content;
		}


		/// <summary>
		/// Transfers processing of the request to the given URL.
		/// </summary>
		/// <param name="url">The URL to transfer processing to.</param>
		public void Transfer(
			string url)
		{
			this.httpContext.Server.Transfer(url, true);
		}


		/// <summary>
		/// Redirects a client to a new URL and specifies the new URL.
		/// </summary>
		/// <param name="url">The target location.</param>
		public void Redirect(
			string url)
		{
			this.httpContext.Response.Redirect(url);
		}


		/// <summary>
		/// Validates data submitted by a client and raises an exception 
		/// if potentially dangerous data is present.
		/// </summary>
		public void ValidateInput()
		{
			this.httpContext.Request.ValidateInput();
		}

		#endregion

		/// <summary>
		/// Gets the URL of the request.
		/// </summary>
		public Uri Url
		{
			get { return this.httpContext.Request.Url; }
		}


		public Uri UrlReferrer
		{
			get { return this.httpContext.Request.UrlReferrer; }
		}


		/// <summary>
		/// Gets the RawUrl of the request (url before any rewriting or redirect)
		/// </summary>
		public string RawUrl
		{
			get { return this.httpContext.Request.RawUrl; }
		}

		#region private members

		/// <summary>
		/// Gets the local collection of parameters.
		/// </summary>
		private NameValueCollection LocalParams
		{
			get
			{
				if (!this.useLocal) {
					this.MakeLocalCollection();
				}

				return this.localParams;
			}
		}


		/// <summary>
		/// Builds the local collection of parameters.
		/// </summary>
		private void MakeLocalCollection()
		{
			if (this.localParams == null) {
				this.localParams = new NameValueCollection();

				this.localParams.Add(this.httpContext.Request.Form);
				this.localParams.Add(this.httpContext.Request.QueryString);

				this.useLocal = true;
			}
		}

		#endregion
	}
}