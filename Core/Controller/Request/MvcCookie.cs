using System;
using System.Collections.Specialized;
using System.Web;
using Triton.Utilities;

namespace Triton.Controller.Request
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>MvcCookie</b> is an adaptor for the underlying cookie implementation to adapt it
	/// for use with <b>MvcRequest</b>.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class MvcCookie
	{
		private readonly HttpCookie httpCookie;


		///<summary>
		///</summary>
		///<param name="name"></param>
		public MvcCookie(
			string name)
		{
			this.httpCookie = new HttpCookie(name);
		}


		public MvcCookie(
			string name,
			string val)
		{
			this.httpCookie = new HttpCookie(name, val);
		}


		public MvcCookie(
			HttpCookie httpCookie)
		{
			this.httpCookie = httpCookie;
		}


		public string this[string name]
		{
			get { return this.httpCookie[name]; }
			set { this.httpCookie[name] = value; }
		}


		/// <summary>
		/// Gets or sets the domain to associate the cookie with. 
		/// </summary>
		public string Domain
		{
			get { return this.httpCookie.Domain; }
			set { this.httpCookie.Domain = value; }
		}


		/// <summary>
		/// Gets or sets the expiration date and time for the cookie.
		/// </summary>
		public DateTime Expires
		{
			get { return this.httpCookie.Expires; }
			set { this.httpCookie.Expires = value; }
		}


		/// <summary>
		/// Gets a value indicating whether a cookie has subkeys.
		/// </summary>
		public bool HasKeys
		{
			get { return this.httpCookie.HasKeys; }
		}


		/// <summary>
		/// Gets or sets the name of a cookie.
		/// </summary>
		public string Name
		{
			get { return this.httpCookie.Name; }
			set { this.httpCookie.Name = value; }
		}


		/// <summary>
		/// Gets or sets the virtual path to transmit with the current cookie.
		/// </summary>
		public string Path
		{
			get { return this.httpCookie.Path; }
			set { this.httpCookie.Path = value; }
		}


		/// <summary>
		/// Gets or sets a value indicating whether to transmit the 
		/// cookie using SSL (that is, over HTTPS only).
		/// </summary>
		public bool Secure
		{
			get { return this.httpCookie.Secure; }
			set { this.httpCookie.Secure = value; }
		}


		/// <summary>
		/// Gets or sets an individual cookie value.
		/// </summary>
		public string Value
		{
			get { return this.httpCookie.Value; }
			set { this.httpCookie.Value = value; }
		}


		/// <summary>
		/// Gets a collection of key-and-value value pairs that are 
		/// contained within a single cookie object.
		/// </summary>
		public NameValueCollection Values
		{
			get { return this.httpCookie.Values; }
		}


		internal HttpCookie HttpCookie
		{
			get { return this.httpCookie; }
		}
	}
}