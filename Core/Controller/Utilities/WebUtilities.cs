using System.Web;
using Triton.Controller.Request;

namespace Triton.Controller.Utilities
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// Utility/helper class for web/http support.
	/// </summary>
	///	<author>Scott Dyke</author>
	public static class WebUtilities
	{

		/// <summary>
		/// Gets the IP address of the client from which the given request originated.
		/// </summary>
		/// <remarks>
		/// This is basically to handle the problem introduced by Netscaler where it
		/// alters the REMOTE_ADDR header (Request's UserHostAddress property), and
		/// places the IP address in the HTTP_CLIENT_IP header.
		/// </remarks>
		/// <param name="request">The <b>HttpRequest</b> to get the origin IP of.</param>
		/// <returns>The IP address of the client from which the given request originated</returns>
		public static string GetIP(
			HttpRequest request)
		{
			//  first try the HTTP_CLIENT_IP header
			string ip = request.ServerVariables["http_client_ip"];

			//  if there is no value in the HTTP_CLIENT_IP header, try REMOTE_ADDR
			if (string.IsNullOrEmpty(ip)) {
				ip = request.ServerVariables["remote_addr"];
			}

			//  Netscaler sometimes puts mutliple, comma-delimited IPs in the header --
			//  so if there's a comma get just the first IP
			if (ip.IndexOf(',') >= 0) {
				ip = ip.Substring(0, ip.IndexOf(','));
			}

			return ip;
		}


		/// <summary>
		/// Gets the IP address of the client from which the given request originated.
		/// </summary>
		/// <remarks>
		/// This is basically to handle the problem introduced by Netscaler where it
		/// alters the REMOTE_ADDR header (Request's UserHostAddress property), and
		/// places the IP address in the HTTP_CLIENT_IP header.
		/// </remarks>
		/// <param name="request">The <b>MvcRequest</b> to get the origin IP of.</param>
		/// <returns>The IP address of the client from which the given request originated</returns>
		public static string GetIP(
			MvcRequest request)
		{
			return request.IP;
		}


		/// <summary>
		/// Gets the logon user of the client request.
		/// </summary>
		/// <param name="request">The <c>HttpRequest</c> to get the user name from.</param>
		/// <returns><c>string</c> The logon user of the client request.</returns>
		public static string GetUserName(
			HttpRequest request)
		{
			return request.ServerVariables["LOGON_USER"];
		}


		/// <summary>
		/// Determines if the <c>fieldName</c> item in <c>request</c> has a value or not. The item has a
		/// value if it is not <c>NULL</c> and a length > 0.
		/// </summary>
		/// <param name="request"><c>HttpRequest</c> object containing the item to check</param>
		/// <param name="fieldName"><c>String</c> containing the item name to check</param>
		/// <returns><c>True</c> if the item specified by <c>fieldName</c> is not <c>NULL</c> and has a
		/// length > 0, <c>false</c> otherwise.</returns>
		public static bool HasValue(
			this HttpRequest request,
			string fieldName)
		{
			return (!string.IsNullOrEmpty(request[fieldName]));
		}


		/// <summary>
		/// Determines if the <c>fieldName</c> item in <c>request</c> has a value or not. The item has a
		/// value if it is not <c>NULL</c> and a length > 0.
		/// </summary>
		/// <param name="request"><c>HttpRequest</c> object containing the item to check</param>
		/// <param name="fieldName"><c>String</c> containing the item name to check</param>
		/// <returns><c>True</c> if the item specified by <c>fieldName</c> is not <c>NULL</c> and has a
		/// length > 0, <c>false</c> otherwise.</returns>
		public static bool HasValue(
			this MvcRequest request,
			string fieldName)
		{
			return (!string.IsNullOrEmpty(request[fieldName]));
		}
	}
}