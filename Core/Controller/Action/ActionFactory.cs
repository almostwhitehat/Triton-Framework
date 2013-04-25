using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using System.Text;
using Common.Logging;
using Triton.Configuration;
using Triton.Support.Collections;

namespace Triton.Controller.Action
{

	#region History

	// History:
	// 05/20/2009	KP	Changed the Logging to use Common.Logging
	// 09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method
	// 12/10/2009	GV	Added a log which action was successfully created

	#endregion

	/// <summary>
	/// Factory class to build <c>Action</c>s.
	/// </summary>
	///	<author>Scott Dyke</author>
	public static class ActionFactory
	{
		private const string ACTION_SUFFIX = "Action";

		/// <summary>
		/// The config section name of the settings for Actions in web.config
		/// </summary>
		private const string SECTION_NAME = "actions";

		/// <summary>
		/// The <c>Cache</c> that keeps track of found actions.
		/// </summary>
		private static readonly Cache typeCache = new Cache(new TimeSpan(1, 1, 1, 1), new TimeSpan(100, 1, 1, 1), CacheType.ABSOLUTE_EXPIRATION);


		/// <summary>
		/// Instantiates the <b>Action</b> with the given name, if available.
		/// </summary>
		/// <param name="actionName">The name of the Action to instantiate.</param>
		/// <returns>The <b>Action</b> with the specified name, or <b>null</b> if no
		/// Action with that name is found.</returns>
		public static IAction Make(
			string actionName)
		{
			IAction action;
			Type type = null;

			//check if the action has been found and cached before
			if (typeCache[actionName] != null) {

				type = typeCache[actionName] as Type;

			} else {
				//  get the namespace and assembly the actions are in
				NameValueCollection config = (NameValueCollection)ConfigurationManager.GetSection(TritonConfigurationSection.SectionName + "/" + SECTION_NAME);

				// making sure that the configuration has been defined
				if (config == null) {
					throw new ConfigurationErrorsException(string.Format("No config settings found for {0}.", TritonConfigurationSection.SectionName + "/" + SECTION_NAME));
				}

				try {
					//  try to get the type for an Action defined in the namespace/assembly from web.config
					for (int k = 0; ((k < config.Count) && (type == null)); k++) {
						type = GetType(config[k], null, actionName);
					}

					//  if the Action was not found in the namespace/assembly defined in web.config,
					//  try the current namespace/assembly -- maybe it's an internal Action
					if (type == null) {
						//  get the namespace of this class
						string nameSpace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
						//  get the assembly name of this class
						string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
						//  try to get the type for an internal Action
						type = GetType(nameSpace, assemblyName, actionName);
					}

					// type is still null or not found, then throw an exception up to callee
					if (type == null) {
						throw new TargetInvocationException("Could not create the action " + actionName + ACTION_SUFFIX + ".", null);
					}

					// try to cache the type, if fails, ignore the exception
					try {
						typeCache.Add(actionName, type);
					} catch (Exception ex) {
						LogManager.GetCurrentClassLogger().Warn(warn => warn("Could not add the action : [{0}] to cache.", actionName), ex);
					}

				} catch (Exception) {

					ILog logger = LogManager.GetCurrentClassLogger();
                    // only build the message if the current class logging level is Info or above
					if (logger.IsInfoEnabled) {
						StringBuilder builder = new StringBuilder();

						builder.AppendFormat("Attempted creation of the action {0}{1} in these namespaces:", actionName, ACTION_SUFFIX);

						foreach (string key in config.Keys) {
							builder.Append(Environment.NewLine + key + " -> " + config[key] + ";");
						}

						builder.Remove(builder.Length - 1, 1);

						builder.Append(".");

						logger.Info(builder.ToString());
					}

					throw;
				}
			}

			try {
				action = (IAction)Activator.CreateInstance(type);

				LogManager.GetCurrentClassLogger().Debug(debug => debug("Successfully created action of type: {0}", type.AssemblyQualifiedName));

			} catch (Exception ex) {
				typeCache.Remove(actionName);
				throw new TargetInvocationException("Could not create the action " + actionName + ACTION_SUFFIX + ".", ex);
			}

			return action;
		}


		/// <summary>
		/// Gets the <b>Type</b> for the action with the given name in the specified namespace
		/// and assembly.
		/// </summary>
		/// <param name="nameSpace">The namespace the action class is defined in.</param>
		/// <param name="assemblyName">The name of the assembly the action class is defined in.</param>
		/// <param name="actionName">The name of the action.</param>
		/// <returns></returns>
		private static Type GetType(
			string nameSpace,
			string assemblyName,
			string actionName)
		{
			//  "nameSpace" may be of the form <namespace>,<assembly> so
			//  if there's a comma, get the separate parts (actionName has to
			//  be appended to nameSpace)
			string[] parts = nameSpace.Split(',');

			//  get the name space and assembly parts
			if (parts.Length == 2) {
				nameSpace = parts[0];
				assemblyName = parts[1];
			}

			//  build the name of the Action object for the given namespace/assembly
			string fullName = string.Format("{0}.{1}{2},{3}", nameSpace, actionName, ACTION_SUFFIX, assemblyName);

			return Type.GetType(fullName);
		}
	}
}