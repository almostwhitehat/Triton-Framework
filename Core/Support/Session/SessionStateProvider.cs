using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Triton.Controller;

namespace Triton.Support.Session
{
	public class SessionStateProvider
	{
		private static ISessionState sessionState = null;

		public static ISessionState GetSessionState()
		{
			if (sessionState == null) { 
				// find the implementation from a configuration entry
				//  get the settings from configuration file
				//ControllerConfigSection config = ConfigurationManager.GetSection("controllerSettings/content") as ControllerConfigSection;

				////  make sure we have the proper configuration info
				//if (config == null) {
				//    throw new ConfigurationErrorsException("Load of controllerSettings/content configuration section failed.");
				//}
				string typeName = "Triton.Web.Support.Session.HttpSessionState, Triton.Extensions.Web";
				if (ConfigurationManager.AppSettings["sessionProvider"] != null) {
					typeName = ConfigurationManager.AppSettings["sessionProvider"];
				}

				Type type = Type.GetType(typeName);

				//  make sure we successfully got the type for the provider to instantiate
				if (type == null) {
					throw new ConfigurationErrorsException(string.Format("Type not found: '{0}'.", ConfigurationManager.AppSettings["sessionProvider"]));
				}

				//  instantiate the new ContentProvider
				sessionState = (ISessionState)Activator.CreateInstance(type);

				if (sessionState == null) {
					throw new ConfigurationErrorsException(string.Format("Instantiation of type '{0}' failed.", ConfigurationManager.AppSettings["sessionProvider"]));
				}
			}

			return sessionState;
		}
	}
}
