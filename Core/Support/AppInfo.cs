using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Triton.Controller.Config;

namespace Triton.Support
{
		/// <summary>
		/// <c>AppInfo</c> is a convenience class for getting information about the environment.
		/// </summary>
		///	<author>Scott Dyke</author>
	public class AppInfo
	{
		/// <summary>
		/// Represents application-level variables typically referenced by Application["key"].
		/// This exists due to the need to reference HttpContext.Current in global.asax.cs' 
		/// Application_Start method, which does not exist at that time if a page hasn't been
		/// referenced yet.
		/// </summary>
		private static readonly Hashtable applicationVariables = Hashtable.Synchronized(new Hashtable());


		protected AppInfo() { }


		/// <summary>
		/// Gets the file system path to the application root directory.
		/// </summary>
		public static string BasePath
		{
			get { return (String)applicationVariables["BasePath"]; }
		}


		/// <summary>
		/// Gets the relative URL to the HTTP controller.
		/// </summary>
		public static string Controller
		{
			get { return (String)applicationVariables["Controller"]; }
		}

		/// <summary>
		/// Represents application-level variables, that would typically be stored in Application["key"].
		/// </summary>
		public static Hashtable ApplicationVariables
		{
			get { return applicationVariables; }
		}
		
	}
}
