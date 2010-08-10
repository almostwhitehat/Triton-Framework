using System.Web;
using Triton.Controller.Config;
using Triton.Utilities.Configuration;

namespace Triton.Controller.Utilities
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <c>LanguageUtilities</c> is a utility class to support site langauge usage.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class LanguageUtilities
	{
		internal const string MVC_COOKIE_COLLECTION = "triton_mvc_cookies";
		internal const string LANGUAGE_COOKIE = "language";


		private LanguageUtilities() {}


		public static string GetLanguage(
			string site)
		{
			HttpRequest request = HttpContext.Current.Request;
			string lang = null;

			//  try to get language setting from cookie
			try {
				if (request.Cookies[MVC_COOKIE_COLLECTION] != null) {
					lang = request.Cookies[MVC_COOKIE_COLLECTION][LANGUAGE_COOKIE];
				}

				//  try to get language setting from request's UserLanguages
// ?? do we want to use this method??
//			if (lang == null) {
//				string userLang = request.UserLanguages[0];
//				if (userLang[2] == '-') {
//					lang = userLang.Substring(0, 2);
//				} else {
//				}
//			}
			} catch {
				int k = 0;
			}

			if (lang == null) {
				XmlConfiguration siteConfig = SitesConfig.GetInstance().GetConfig("sites", site.ToUpper());
				if (siteConfig != null) {
					lang = siteConfig.GetValue("//defaultLanguage");
				}
			}

			HttpContext.Current.Response.Cookies[MVC_COOKIE_COLLECTION][LANGUAGE_COOKIE] = lang;


			return lang;
		}
	}
}