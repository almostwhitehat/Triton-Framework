using System;
using System.Reflection;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Controller.Request;
using Triton.Model;
using Triton.Logic.Support;
using Triton.Utilities.Reflection;
using Triton.CodeContracts;

namespace Triton.Logic {


/// <summary>
/// Resets a singleton, forcing it to re-initialize on its next usage.
/// </summary>
/// <remarks>
/// This action is designed for SingletonBase singletons but will also work
/// with other singletons that meet the following criteria:
/// <ol>
/// <li>Must have a static bool Exists property that indicates if the instance of
///		the singleton has be instantiated.</li>
///	<li>Must have a reset method to force re-initialization of the singleton.</li>
/// </ol>
/// <para>Returned events:</para>
/// <c>ok</c> - the singleton was successfully reset.<br/>
/// <c>error</c> - an error occurred while attempting to reset the singleton.<br/>
/// </remarks>
///	<author>Scott Dyke</author>
///	<created>11/30/10</created>
public class ResetSingletonAction : IAction
{
	private const char	SINGLETON_NAME_DELIMITER	= '|';


	public ResetSingletonAction()
	{
		ResetMethodNameIn = "Refresh";		//  deault value, for SingletonBase derived singletons
	}


	/// <summary>
	/// Gets or sets the fully qualified name(s) of the singleton(s) to reset.
	/// </summary>
	public string SingletonNameIn
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the name of the request parameter containing the fully
	/// qualified name(s) of the singleton(s) to reset.
	/// </summary>
	public string SingletonNameParamIn
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the name of the singleton's reset method.
	/// </summary>
	public string ResetMethodNameIn
	{
		get;
		set;
	}


	#region IAction Members

	public string Execute(
		TransitionContext context)
	{
		string retEvent = Events.Error;
		string currentName = "";
		MvcRequest request = context.Request;

		try {
			ActionContract.Requires<ApplicationException>(!(string.IsNullOrEmpty(SingletonNameIn) && string.IsNullOrEmpty(SingletonNameParamIn)),
					"One of SingletonNameIn or SingletonNameParamIn must be provided.");

					//  get the name(s) of the singleton(s) to reset
			string singletonNames = SingletonNameIn ?? request[SingletonNameParamIn];

			if (string.IsNullOrEmpty(singletonNames)) {
				throw new ApplicationException("No singleton name(s) given.");
			}

					//  for each singleton specified...
			foreach (string singletonName in singletonNames.Split(SINGLETON_NAME_DELIMITER)) {
				currentName = singletonName;
						//  get the singleton's type (for reflection)
				Type singletonType = Type.GetType(singletonName);

				if (singletonType == null) {
					throw new NullReferenceException(string.Format(
							"Could not find type for '{0}'.  Are you missing an assembly reference?", singletonName));
				}

						//  get the singleton's "Exists" property
				PropertyInfo existsProperty = singletonType.GetProperty("Exists", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

				if (existsProperty == null) {
					throw new ApplicationException("Could not obtain Exists property for singleton.");
				}

						//  check to see if the singleton's instance exists
				bool exists = (bool)existsProperty.GetValue(null, null);

						//  if the singleton's instance does exist...
				if (exists) {
							//  get the Instance property
					//PropertyInfo instanceProperty = singletonType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

					//if (instanceProperty == null) {
					//    throw new ApplicationException("Could not obtain Instance property for singleton.");
					//}

					//        //  get the singleton instance
					//object singleton = instanceProperty.GetValue(null, null);

					//if (singleton == null) {
					//    throw new ApplicationException("Could not get instance of singleton.");
					//}

							//  get the singleton's reset method
					MethodInfo resetMethod = singletonType.GetMethod(ResetMethodNameIn, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static);

					if (resetMethod == null) {
						throw new MissingMethodException(string.Format(
								"Could not find '{0}' method for singleton.", ResetMethodNameIn));
					}

							//  call the reset method
					resetMethod.Invoke(null, null);
				}
			}
			
		} catch (Exception e) {
			ILog logger = LogManager.GetCurrentClassLogger();
			logger.Error(error => error(string.Format(
					"Error occurred resetting the singleton '{0}'.", currentName)), e);
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
