using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using Common.Logging;
using Triton.Configuration;
using Triton.Validator.Model.Rules;

namespace Triton.Validator.Model
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// The <b>ValidationRuleFactory</b> implements a <i>Factory</i> pattern for
	/// creating <b>IValidationRule</b>s.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class ValidationRuleFactory
	{
		/// <summary>
		/// The config section name of the settings for formatters in web.config
		/// </summary>
		private const string SECTION_NAME = "validationRules";

		private const string RULE_SUFFIX = "Rule";


		/// <summary>
		/// Private constructor to prevent instantiation.
		/// </summary>
		private ValidationRuleFactory() {}


		/// <summary>
		/// Instantiates the <b>IValidationRule</b> with the given name, if available.
		/// </summary>
		/// <param name="ruleName">The name of the IValidationRule to instantiate.</param>
		/// <returns>The <b>IValidationRule</b> with the specified name, or <b>null</b> if no
		/// IValidationRule with that name is found.</returns>
		public static IValidationRule Make(
			string ruleName)
		{
			IValidationRule rule = null;

			try {
				//  get the namespace and assembly the rules are in
				NameValueCollection config = (NameValueCollection)ConfigurationManager.GetSection(TritonConfigurationSection.SectionName + "/" + SECTION_NAME);

				if (config == null) {
					throw new Exception(string.Format("No config settings found for {0}.", TritonConfigurationSection.SectionName + "/" + SECTION_NAME));
				}

				//  try to get the type for an IValidationRule defined in the namespace/assembly from web.config
				Type type = null;
				for (int k = 0; ((k < config.Count) && (type == null)); k++) {
					type = GetType(config[k], null, ruleName);
				}

				//  if the IValidationRule was not found in the namespace/assembly defined in web.config,
				//  try the current namespace/assembly -- maybe it's an internal IValidationRule
				if (type == null) {
					//  get the namespace of this class
					string nameSpace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
					//  get the assembly name of this class
					string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
					//  try to get the type for an internal IValidationRule
					type = GetType(nameSpace, assemblyName, ruleName);
				}

				rule = (IValidationRule) Activator.CreateInstance(type);
			} catch (Exception e) {
				LogManager.GetLogger(typeof (ValidationRuleFactory)).Error(
					errorMessage => errorMessage(string.Format("ValidationRuleFactory.Make: rule = {0}", ruleName), e));
			}

			return rule;
		}


		/// <summary>
		/// Gets the <b>Type</b> for the rule with the given name in the specified namespace
		/// and assembly.
		/// </summary>
		/// <param name="nameSpace">The namespace the rule class is defined in.</param>
		/// <param name="assemblyName">The name of the assembly the rule class is defined in.</param>
		/// <param name="ruleName">The name of the rule.</param>
		/// <returns></returns>
		private static Type GetType(
			string nameSpace,
			string assemblyName,
			string ruleName)
		{
			//  "nameSpace" may be of the form <namespace>,<assembly> so
			//  if there's a comma, get the separate parts (ruleName has to
			//  be appended to nameSpace)
			string[] parts = nameSpace.Split(',');

			//  get the name space and assembly parts
			if (parts.Length == 2) {
				nameSpace = parts[0];
				assemblyName = parts[1];
			}

			//  build the name of the IValidationRule object for the given namespace/assembly
			string fullName = string.Format("{0}.{1}{2},{3}", nameSpace, ruleName, RULE_SUFFIX, assemblyName);

			return Type.GetType(fullName);
		}
	}
}