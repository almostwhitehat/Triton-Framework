using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using Triton.Controller.Utilities;

namespace Triton.Controller.Request
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>MvcXmlRequest</b> is the implementation of the <b>MvcRequest</b> interface
	/// for XML requests.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class MvcXmlRequest : MvcRequest
	{
		private const string LOGGER_NAME = "ControllerLogger";

		/// <summary>
		/// The name in the <i>Items</i> property of the XmlDocument containing the initial request.
		/// </summary>
		public const string REQUEST_XML = "requestXml";

		/// <summary>
		/// The name in the <i>Items</i> property of the XmlDocument containing the initial request.
		/// </summary>
		public const string RESPONSE_XML = "responseXml";

		private readonly HttpContext httpContext;
		private readonly string ip;

		/// <summary>
		/// Represents custom parameters.
		/// </summary>
		private readonly NameValueCollection parameters = new NameValueCollection();

		private readonly string userName;


		/// <summary>
		/// Constructs a <b>MvcXmlRequest</b> from the given HttpContext.
		/// </summary>
		/// <param name="httpContext">The <b>HttpContext</b> to construct the MvcXmlRequest from.</param>
		public MvcXmlRequest(
			HttpContext httpContext)
		{
			byte[] requestBytes = new byte[httpContext.Request.InputStream.Length];

			this.httpContext = httpContext;
			// NOTE: we read directly from the request input stream rather than using a StreamReader
			// because that will close the underlying stream for further operations after calling 
			// close or garbage collection
			httpContext.Request.InputStream.Read(requestBytes, 0, requestBytes.Length);
			httpContext.Request.InputStream.Seek(0, SeekOrigin.Begin);

			string content = Encoding.ASCII.GetString(requestBytes);
			this.LoadXml(content);

			this.ip = WebUtilities.GetIP(httpContext.Request);
			this.userName = WebUtilities.GetUserName(httpContext.Request);
		}


		/// <summary>
		/// Overloaded contstuctor that receives an additional string parameter 
		/// which is the string representation of the HttpContext so we don't have 
		/// to load it twice.
		/// </summary>
		/// <param name="httpContext">The <b>HttpContext</b> to construct the MvcXmlRequest from.</param>
		/// <param name="contextStreamData"><b>HttpContext.Request.InputStream</b>'s full content.</param>
		public MvcXmlRequest(
			HttpContext httpContext,
			string contextStreamData)
		{
			this.httpContext = httpContext;

			this.LoadXml(contextStreamData);

			this.ip = WebUtilities.GetIP(httpContext.Request);
		}

		#region MvcRequest Members

		/// <summary>
		/// Gets or Sets the parameter with the given name.
		/// Indexer for a <b>MvcXmlRequest</b>.  
		/// </summary>
		public string this[string paramName]
		{
			get { return this.parameters[paramName]; }
			set { this.parameters[paramName] = value; }
		}


		/// <summary>
		/// Returns a <b>NameValueCollection</b> of all parameters in the request, including custom ones.
		/// </summary>
		public NameValueCollection Params
		{
			get { return this.parameters; }
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
		/// Gets the collection of client-uploaded files.
		/// </summary>
		public MvcPostedFileCollection Files
		{
			get
			{
// TODO: implement this.
				return null;
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
			// TODO:  Add MvcXmlRequest.GetCookie implementation
			return null;
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
			// TODO:  Add MvcXmlRequest.GetResponseCookie implementation
			return null;
		}


		/// <summary>
		/// Sets a <b>MvcCookie</b> in the response.  If the cookie already
		/// exists, its information is updated, if not, it is created.
		/// </summary>
		/// <param name="cookie">The <b>MvcCookie</b> to set in the response.</param>
		public void SetResponseCookie(
			IMvcCookie cookie)
		{
			// TODO:  Add MvcXmlRequest.SetResponseCookie implementation
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
			// TODO:  Add MvcXmlRequest.Execute implementation
			return null;
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


		public void ValidateInput()
		{
			this.httpContext.Request.ValidateInput();
		}

		#endregion

		/// <summary>
		/// Loads the information into the request from the given XML string.
		/// </summary>
		/// <param name="xmlData">An XML string containing the inforamtion to load
		///		into the request.</param>
		private void LoadXml(
			string xmlData)
		{
			XmlDocument doc = new XmlDocument();

			try {
				doc.LoadXml(xmlData);

				this.httpContext.Items[REQUEST_XML] = doc;

				foreach (XmlNode param in doc.SelectNodes("request//parameters/parameter")) {
					this.parameters.Add(param.Attributes["name"].Value, param.InnerText);
				}

				//  get the "st" and "e" attributes from the request node and include them
				//  as parameters
				XmlNode req = doc.SelectSingleNode("request");
				if (req != null) {
					string[] parms = new[] {"st", "e"};
					for (int k = 0; k < parms.Length; k++) {
						if (req.Attributes[parms[k]] != null) {
							this.parameters.Add(parms[k], req.Attributes[parms[k]].Value);
						}
					}
				}
			} catch (Exception e) {
				///			Logger.GetLogger(LOGGER_NAME).Error(string.Format("LoadXml: data= {0}", xmlData), e);
				throw (e);
			}
		}
	}
}