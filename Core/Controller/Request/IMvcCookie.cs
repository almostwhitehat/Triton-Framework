using System;
using System.Collections.Specialized;

namespace Triton.Controller.Request
{
	public interface IMvcCookie
	{
		string this[string name] { get; set; }

		/// <summary>
		/// Gets or sets the domain to associate the cookie with. 
		/// </summary>
		string Domain { get; set; }

		/// <summary>
		/// Gets or sets the expiration date and time for the cookie.
		/// </summary>
		DateTime Expires { get; set; }

		/// <summary>
		/// Gets a value indicating whether a cookie has subkeys.
		/// </summary>
		bool HasKeys { get; }

		/// <summary>
		/// Gets or sets the name of a cookie.
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Gets or sets the virtual path to transmit with the current cookie.
		/// </summary>
		string Path { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to transmit the 
		/// cookie using SSL (that is, over HTTPS only).
		/// </summary>
		bool Secure { get; set; }

		/// <summary>
		/// Gets or sets an individual cookie value.
		/// </summary>
		string Value { get; set; }

		/// <summary>
		/// Gets a collection of key-and-value value pairs that are 
		/// contained within a single cookie object.
		/// </summary>
		NameValueCollection Values { get; }
	}
}