using System;
using System.Linq;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml;
using Common.Logging;
using Triton.Utilities;
using Triton.Utilities.Reflection;
using Triton.Validator.Model.Rules;
using Triton.Support;

namespace Triton.Validator.Model
{

	#region History

	// History:
	//	10/15/2009	KP	Merged changes from Catalyst to Triton, allowing more than one validator.config file.

	#endregion

	/// <summary>
	/// The <b>ValidatorManager</b> manages the availabe <b>Validator</b>s.  A
	/// <b>Validator</b> is obtained using <b>ValidatorManager</b>'s <c>GetValidator</c>
	/// method.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class ValidatorManager
	{
		private static ValidatorManager instance;
		private static object syncRoot = new object();
		private static Hashtable validatorHash = Hashtable.Synchronized(CollectionsUtil.CreateCaseInsensitiveHashtable());
		private readonly ILog logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Private constructor to enforce singleton pattern.
		/// </summary>
		private ValidatorManager()
		{
			this.LoadValidators();
		}


		/// <summary>
		/// Gets the singleton instance of the <b>ValidatorManager</b>.
		/// </summary>
		/// <returns>The singleton instance of the <b>ValidatorManager</b>.</returns>
		public static ValidatorManager GetInstance()
		{
			if (instance == null) {
				//  ensure only one thread is able to instantiate an instance at a time.
				lock (syncRoot) {
					if (instance == null) {
						instance = new ValidatorManager();
					}
				}
			}

			return instance;
		}


		/// <summary>
		/// Gets the <b>Validator</b> with the given name, or <c>null</c> if no
		/// such validator exists.
		/// </summary>
		/// <param name="name">The name of the <b>Validator</b> to get.</param>
		/// <returns>The <b>Validator</b> with the given name.</returns>
		public Rules.Validator GetValidator(
			string name)
		{
			return (Rules.Validator) validatorHash[name];
		}


		/// <summary>
		/// Loads the <b>Validator</b>s from the config file.
		/// </summary>
		private void LoadValidators()
		{
			XmlDocument doc = new XmlDocument();

			string rootPath = AppInfo.BasePath;
			
			string validatorsConfigPath = System.Configuration.ConfigurationManager.AppSettings["validatorsConfigPath"];

			logger.Debug(debugMessage => debugMessage("ValidatorManager - starting load."));

			if (validatorsConfigPath != "") {
				LoadValidatorConfigFile(rootPath + validatorsConfigPath);
			}

			try {
				String[] files = System.IO.Directory.GetFiles(rootPath + System.Configuration.ConfigurationManager.AppSettings["validatorsBasePath"], "*.validators.config");
				foreach (string file in files) {
					LoadValidatorConfigFile(file);
				}

				logger.Debug(debugMessage => debugMessage("ValidatorManager - load completed."));
			} catch (Exception e) {
				logger.Error(errorMessage => errorMessage("ValidatorManager - error loading validators: "), e);
			}
		}


		private void LoadValidatorConfigFile(
			string filename)
		{
			XmlDocument doc = new XmlDocument();

			try {
				logger.Debug(debugMessage => debugMessage("ValidatorManager - starting load of config file ({0}).", filename));

				//  load the xml from the config file
				doc.Load(filename);

				XmlNodeList validatorsXml = doc.DocumentElement.SelectNodes("/validators/validator");

				foreach (XmlNode validatorNode in validatorsXml) {
					Rules.Validator validator = BuildValidator(validatorNode);

					validatorHash.Add(validator.Name, validator);
				}

				logger.Debug(debugMessage => debugMessage("ValidatorManager - load of config file({0}) completed.", filename));
			} catch (Exception e) {
				logger.Error(errorMessage => errorMessage("ValidatorManager - error loading validator config file({0}): ", filename), e);
			}
		}


		/// <summary>
		/// Builds a <b>Validator</b> and its rules from the given XML node.
		/// </summary>
		/// <param name="validatorNode">The XML node containing the definition
		///			for the <b>Validator</b> and its rules.</param>
		/// <returns></returns>
		private Rules.Validator BuildValidator(
			XmlNode validatorNode)
		{
			string name = validatorNode.Attributes["name"].Value;
			Rules.Validator validator = new Rules.Validator(name);

			GetChildren(validator, validatorNode);

			return validator;
		}


		/// <summary>
		/// Builds the child rules for the given rule from the given XML node.
		/// </summary>
		/// <param name="parent">The rule to build the children for.</param>
		/// <param name="parentNode">The XML node containing the definition(s)
		///			of the chlid rules.</param>
		private void GetChildren(
			IValidationRule parent,
			XmlNode parentNode)
		{
			foreach (XmlNode ruleNode in parentNode) {
				string nodeName = ruleNode.Name;
// null??
				//  if the rule has properties, set them on the rule object
				if (nodeName == "properties") {
					foreach (XmlNode paramNode in ruleNode.ChildNodes) {
						string attrName = StringUtilities.Capitalize(paramNode.Name);
						if (ReflectionUtilities.HasProperty(parent, attrName)) {
							ReflectionUtilities.SetPropertyValue(parent, attrName, paramNode.InnerText);
						}
					}

//			} else if (nodeName == "validator") {
				} else {
					IValidationRule rule = ValidationRuleFactory.Make(StringUtilities.Capitalize(nodeName));

					if (rule != null) {
						if ((ruleNode.Attributes != null) && (ruleNode.Attributes.Count > 0)) {
							NameValueCollection attributes = new NameValueCollection();
							foreach (XmlAttribute attr in ruleNode.Attributes) {
								attributes.Add(attr.Name, attr.Value);
							}

							ReflectionUtilities.Deserialize(rule, attributes);
						}


					//if (rule != null) {
					//    //  if there are any attributes on the rule node, try setting
					//    //  the corresponding property of the rule object
					//    foreach (XmlAttribute attr in ruleNode.Attributes) {
					//        string attrName = StringUtilities.Capitalize(attr.Name);

					//        //  this is really hokey, but it works for now
					//        if ((attrName == "ErrorId") && ReflectionUtilities.HasProperty(rule, "ErrorId")) {
					//            try {
					//                //								ReflectionUtilities.SetPropertyValue(rule, "ErrorId", long.Parse(attr.Value));
					//                ((BaseRule)rule).ErrorId = long.Parse(attr.Value);
					//            } catch {
					//                // TODO: log the failure?
					//            }
					//        } else if (ReflectionUtilities.HasProperty(rule, attrName)) {
					//            ReflectionUtilities.SetPropertyValue(rule, attrName, attr.Value);
					//        }
					//    }
								//  get the children of the rule
					    GetChildren(rule, ruleNode);
								//  add the rule to the parent's list of children
					    parent.Add(rule);
					}
					//  TODO: else log it?
				}
			}
		}


		/// <summary>
		/// Resets the cached validation rules.
		/// </summary>
		public static void Reset()
		{
			instance = null;
			validatorHash.Clear();
		}
	}
}