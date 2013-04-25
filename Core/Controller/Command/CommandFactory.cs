using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using Common.Logging;
using Triton.Configuration;
using Triton.Controller.Request;

namespace Triton.Controller.Command
{

	#region History

	// History:
	// 5/30/2009	KP	Changed the Logging to use Common.Logging
	// 09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method

	#endregion

	/// <summary>
	/// <b>CommandFactory</b> is the factory class for building Command instances.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class CommandFactory
	{
		private const string ACTION_PARAM = "a";

		private const string COMMAND_SECTION_NAME = "commands";
		/// <summary>
		/// The "path" of the settings for Commands in web.config
		/// </summary>
		//private const string CONFIG_PATH = TritonConfigurationSection.SECTION_NAME + "/commands";
		//        //  for backward compatibility (for now)
		//private const string ALT_CONFIG_PATH = "triton/commands";

		private const string THING_PARAM = "t";


		/// <summary>
		/// Makes a Command object based on the given set of parameters.
		/// </summary>
		/// <remarks>
		/// This methods creates a new command object based on the requested
		/// <i>action</i> and <i>thing</i> (or type), the target of the action.
		/// The action is specified by the <b>a</b> parameter and the thing by
		/// the <b>t</b> parameter.<br>
		/// For example, the parameters <c>a=get</c> and <c>t=customer</c> will
		/// cause the creation of a GetCustomerCommand <c>Command</c>.
		/// </remarks>
		/// <param name="request">The parameter collection containing the "a" and
		/// "t" parameters used to create the appropriate <c>Command</c> object.</param>
		/// <returns>A <c>Command</c> for performing the requested action, or
		/// <c>UnknownCommand</c> if the requested command does not exist.</returns>
		public static Command Make(
			MvcRequest request)
		{
			//  get the action being requested
			String action = request[ACTION_PARAM];
			//  get the thing the action is to be performed on
			String thing = request[THING_PARAM];

			return Make(action, thing);
		}


		/// <summary>
		/// Makes a Command object for the given action and thing.
		/// </summary>
		/// <param name="action">The action to be performed.</param>
		/// <param name="thing">The thing that is the target of the action.</param>
		/// <returns>A <c>Command</c> for performing the requested action, or
		///			<c>UnknownCommand</c> if the requested command does not exist.</returns>
		public static Command Make(
			String action,
			String thing)
		{
			return Make(Capitalize(action) + Capitalize(thing) + "Command");
		}


		/// <summary>
		/// Makes a Command object for the given action and thing.
		/// </summary>
		/// <param name="commandName">The name of the Command to make.</param>
		/// <returns>A <c>Command</c> for performing the requested action, or
		///			<c>UnknownCommand</c> if the requested command does not exist.</returns>
		public static Command Make(
			String commandName)
		{
			Command command = new UnknownCommand();

			try {
				//  get the namespace and assembly the Commands are in from web.config
				NameValueCollection config = (NameValueCollection)ConfigurationManager.GetSection(TritonConfigurationSection.SectionName + "/" + COMMAND_SECTION_NAME);

				if (config == null) {
					throw new Exception(string.Format("No config settings found for {0}.", TritonConfigurationSection.SectionName + "/" + COMMAND_SECTION_NAME));
				}

				//  try to get the type for the Command based on the sources
				//  defined in web.config
				Type type = null;
				for (int k = 0; ((k < config.Count) && (type == null)); k++) {
					type = GetType(config[k], null, commandName);
				}

				//  if the Command was not found in the namespace/assembly defined in web.config,
				//  try the current namespace/assembly -- maybe it's an internal Command
				if (type == null) {
					//  get the namespace of this class
					string nameSpace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
					//  get the assembly name of this class
					string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
					//  try to get the type for an internal Command
					type = GetType(nameSpace, assemblyName, commandName);
				}

				command = (Command) Activator.CreateInstance(type);
			} catch (Exception e) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("CommandFactory.Make: command = {0}", commandName), e);
			}

			return command;
		}


		/// <summary>
		/// Gets the <b>Type</b> for the command with the given name in the specified namespace
		/// and assembly.
		/// </summary>
		/// <param name="nameSpace">The namespace the command class is defined in.</param>
		/// <param name="assemblyName">The name of the assembly the command class is defined in.</param>
		/// <param name="cmdName">The name of the command.</param>
		/// <returns></returns>
		private static Type GetType(
			string nameSpace,
			string assemblyName,
			string cmdName)
		{
			//  "nameSpace" may be of the form <namespace>,<assembly> so
			//  if there's a comma, get the separate parts (cmdName has to
			//  be appended to nameSpace)
			string[] parts = nameSpace.Split(',');

			//  get the name space and assembly parts
			if (parts.Length == 2) {
				nameSpace = parts[0];
				assemblyName = parts[1];
			}

			//  build the name of the Command object for the given namespace/assembly
			string fullName = string.Format("{0}.{1},{2}", nameSpace, cmdName, assemblyName);

			return Type.GetType(fullName);
		}


		/// <summary>
		/// Capitalizes the given string.  I.e., 
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		private static String Capitalize(
			String str)
		{
			String retVal = null;

			if (str != null) {
				retVal = str.Substring(0, 1).ToUpper() + str.Substring(1).ToLower();
			}

			return retVal;
		}
	}
}