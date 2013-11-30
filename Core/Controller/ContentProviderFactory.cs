using System;
using System.Configuration;
using Triton.Controller.Request;
using Triton.Controller.StateMachine;
using Triton.Configuration;

namespace Triton.Controller
{

	#region History

	// History:
	//   6/19/09 - SD -	Added support for instantiating the ContentProvider based on the newly
	//					added ContentProvider property on Transition.  If a ContentProvider is
	//					specified on the transition, that takes precedence over the other methods.

	#endregion

	/// <summary>
	/// <b>ContentProviderFactory</b> is the Factory class for making <b>ContentProviders</b>.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class ContentProviderFactory
	{
		/// <summary>
		/// The name of the attribute onthe start state of the TransitionContext to use
		/// to get the name of the ContentProvider to use for rendering the results.
		/// </summary>
		private const string CONTENT_PROVIDER_ATTRIBUTE = "contentProvider";


		/// <summary>
		/// Instantiates a new <b>ContentProvider</b> for the given conext.
		/// </summary>
		/// <param name="context">The context to make a <b>ContentProvider</b> for.</param>
		/// <returns>A <b>ContentProvider</b> to generate content for the given conext.</returns>
		// TODO:
		//  The rules for determining the type of ContentProvider to make should come from the
		//  config file, not be hard-coded here.  Question is, how to distinguish from the request
		//  whether to use html, xml, soap, ajax, json, etc. provider and how to define that
		//  in config.
		public static ContentProvider Make(
			TransitionContext context)
		{
			string name = null;
			ContentProvider cp;

			//  determine the type of ContentProvider to use based on:
			//  1. a specific ContentProvider specified on the transition from the start state
			//  2. a specific ContentProvider specified in the start state (depricated)
			//  3. the request type
			//  (see TODO above)
			if ((context.StartState != null) && (context.StartEvent != null)) {
						//  get the transition for the start state and event
				Transition trans = context.StartState.GetTransition(context.StartEvent);
				if ((trans != null) && (trans.ContentProvider != null)) {
					name = trans.ContentProvider;
				}
			}
			
					//  if we didn't get the ContentProvider name from the transition, try alternate methods
			if (name == null) {
						//  NOTE: state based ContentProvider declaration is depricated.
						//  This remains only for backward compatibility.
						//  if a specific ContentProvider is not specifid, determine it from the request type
				if ((context.StartState is BaseState)
						&& (((BaseState)context.StartState)[CONTENT_PROVIDER_ATTRIBUTE] != null)) {
					name = ((BaseState)context.StartState)[CONTENT_PROVIDER_ATTRIBUTE];

					//  if a specific ContentProvider is not specifid, determine it from the request type
				} else if (context.Request.ToString().Contains("MvcHttpRequest")) {
					name = "html";
				} else if (context.Request.ToString().Contains("MvcXmlRequest")) {
					name = "xml";
				} else if (context.Request.ToString().Contains("MvcCoreRequest")) {
					name = "simple";
				}
			}

			//  get the settings from config file
			ControllerConfigSection config = ConfigurationManager.GetSection(
					TritonConfigurationSection.SectionName + "/content") as ControllerConfigSection;

			//  make sure we have the proper config info
			if (config == null) {
				throw new ConfigurationErrorsException("Load of triton/content config section failed.");
			}
			if (config.ContentProviders[name] == null) {
				throw new ConfigurationErrorsException(
					string.Format("No contentProvider found for '{0}'.", name));
			}

			//  get the type of the provider
			string className = config.ContentProviders[name].Type;

			Type type = Type.GetType(className);

			//  make sure we successfully got the type for the provider to instantiate
			if (type == null) {
				throw new ConfigurationErrorsException(string.Format("Type not found: '{0}'.", className));
			}

			//  instantiate the new ContentProvider
			cp = (ContentProvider)Activator.CreateInstance(type);

			if (cp == null) {
				throw new ConfigurationErrorsException(string.Format("Instantiation of type '{0}' failed.", className));
			}

			cp.Init(context);

			return cp;
		}
	}
}