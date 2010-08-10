using System;
using System.Collections.Specialized;
using System.Configuration;
using Common.Logging;
using Triton.Controller.Request;

namespace Triton.Controller.Format
{

	#region History

	// History:
	// 05/31/2009	KP Changed the logging to Common.Logging
	// 08/12/2009	GV	Rename of BizAction back to Action
	// 09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method

	#endregion

	/// <summary>
	/// <b>FormatterFactory</b> is the factory class for building specific <b>Formatter</b>s.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class FormatterFactory
	{
		/// <summary>
		/// The "path" of the settings for Formatters in web.config
		/// </summary>
		private const string CONFIG_PATH = "controllerSettings/formatters";


		/// <summary>
		/// Makes a <c>Formatter</c> object for the given name.
		/// </summary>
		/// <param name="formatterName">The name of the <c>Formatter</c> to create.</param>
		/// <param name="request">The <c>MvcRequest</c> that is used.</param>
		/// <returns>A <c>Formatter</c> of the specified name.</returns>
		public static Formatter Make(
			string formatterName,
			MvcRequest request)
		{
			Formatter formatter = null;
			string fullName = "";

			try {
				//  get the namespace and assembly the actions are in
				NameValueCollection config = (NameValueCollection) ConfigurationSettings.GetConfig(CONFIG_PATH);
				string assemblyName = config["assembly"];
				string nameSpace = config["namespace"];

				//  build the name of the Action object for the given request
				fullName = string.Format("{0}.{1},{2}", nameSpace, formatterName, assemblyName);

				formatter = (Formatter) Activator.CreateInstance(Type.GetType(fullName));
				formatter.Request = request;
			} catch (Exception e) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("FormatterFactory.Make: formatter = {0} : {1} ", fullName, e));
			}

			return formatter;
		}
	}
}