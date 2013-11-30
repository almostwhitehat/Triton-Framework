using System.Web;
using Triton.Controller.Config;
using Triton.Controller.Request;
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
			MvcRequest request,
			string site)
		{
			//HttpRequest request = HttpContext.Current.Request;
			string lang = null;

			//  try to get language setting from cookie
			try {
				if (request.GetCookie(MVC_COOKIE_COLLECTION) != null) {
					lang = request.GetCookie(MVC_COOKIE_COLLECTION)[LANGUAGE_COOKIE];
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

			// TODO: determine how to put language back in as a cookie
			//request.[MVC_COOKIE_COLLECTION][LANGUAGE_COOKIE] = lang;


			return lang;
		}
	}
}