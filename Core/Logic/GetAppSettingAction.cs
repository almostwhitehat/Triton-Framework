using System;
using System.Configuration;
using System.Collections.Generic;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Controller.Request;
using Triton.Logic.Support;
using Triton.CodeContracts;

namespace Triton.Logic {


/// <summary>
/// Action to get a value from the appSettings of the configuration file and
/// return in the specified request parameter and/or request item.
/// </summary>
/// <author>Scott Dyke</author>
public class GetAppSettingAction : IAction
{


	/// <summary>
	/// Gets or sets the name of the appSetting in the config file to get.
	/// </summary>
	public string AppSettingNameIn
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the name of the request parameter to put the value of
	/// the appSetting into.
	/// </summary>
	public string ParamNameOut
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the name of in Request.Items to place the value of
	/// the appSetting into.
	/// </summary>
	public string ItemNameOut
	{
		get;
		set;
	}


	#region IAction Members

	public string Execute(
		TransitionContext context)
	{
		string retEvent = Events.Error;
		MvcRequest request = context.Request;

		try {
			ActionContract.Requires<ApplicationException>(!string.IsNullOrEmpty(AppSettingNameIn),
					"No app setting name given in the AppSettingNameIn attribute.");
			ActionContract.Requires<ApplicationException>(!(string.IsNullOrEmpty(ParamNameOut) && string.IsNullOrEmpty(ItemNameOut)),
					"One of ParamNameOut or ItemNameOut is required.");
			ActionContract.Requires<ApplicationException>(!string.IsNullOrEmpty(ConfigurationManager.AppSettings[AppSettingNameIn]),
					string.Format("No config setting found for '{0}'.", AppSettingNameIn));

			string val = ConfigurationManager.AppSettings[AppSettingNameIn];

			if (!string.IsNullOrEmpty(ParamNameOut)) {
				request[ParamNameOut] = val;
			}
			if (!string.IsNullOrEmpty(ItemNameOut)) {
				request.Items[ItemNameOut] = val;
			}

			retEvent = Events.Ok;
		} catch (Exception e) {
			ILog logger = LogManager.GetCurrentClassLogger();
			logger.Error(error => error("Error occurred getting app setting."), e);
		}

		return retEvent;
	}

	#endregion


	#region Nested type: Events

	public class Events
	{
		public static string Ok
		{
			get {
				return EventNames.OK;
			}
		}

		public static string Error
		{
			get {
				return EventNames.ERROR;
			}
		}
	}

	#endregion
}
}
