using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml;
using Triton.Controller.StateMachine;
using Common.Logging;
using Triton.Support;

namespace Triton.Model.Dao
{

	#region History

	// History:
	// 6/4/2009		KP	Changed the logging to Common.Logging
	//   6/22/09 - SD -	Added support for identifying the ContentProvider to use to render the
	//					results on the transition

	#endregion

	/// <summary>
	/// <b>ConfigSmStateDao</b> implements the <b>IStateMachineStatesDao</b> interface to load
	/// state/transition information from an XML config file.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class ConfigSmStateDao : IStateMachineStatesDao
	{
		private const string ACTION_ATTRIBUTE = "action";
		private const string EXCLUDE_PARAMS_ATTRIBUTE = "publishKeyExcludeParams";
		private const string ID_ATTRIBUTE = "id";
		private const string NAME_ATTRIBUTE = "name";
		private const string PAGE_ATTRIBUTE = "page";
		private const string PUBLISH_ATTRIBUTE = "publish";
		private const string SECTION_ATTRIBUTE = "section";
		private const string SITE_ATTRIBUTE = "site";
		private const string TRANSITION_GROUP_NAME = "transitionGroup";
		private const string TYPE_ATTRIBUTE = "type";

		#region IStateMachineStatesDao Members

		/// <summary>
		/// Loads the state/tranistion information for an XML config file.
		/// </summary>
		/// <remarks>
		/// The path to the config file to load is specified in the application's config file
		/// appSettings section, with the "statesConfigPath" setting.
		/// </remarks>
		/// <returns>A <b></b> object with the state/transition information loaded from the
		///			config file.</returns>
		public StateInfo LoadStates()
		{
			List<IState> states = new List<IState>();
			StateInfo info = new StateInfo();

			try {
				//  get the path to the states.config file from web.config
				String statesConfigPath = ConfigurationManager.AppSettings["statesConfigPath"];

				if (statesConfigPath == null) {
					throw new Exception("No states.config path in config file.");
				}

				//  make the XmlDocument object for the states.config file & load it
				XmlDocument stateSettings = new XmlDocument();
				stateSettings.Load(AppInfo.BasePath + statesConfigPath);

				//======================================================
				//  load the transition groups
				//======================================================

				//  get the "transitiongroup" nodes
				XmlNodeList transGroupNodes = stateSettings.SelectNodes("/states/" + TRANSITION_GROUP_NAME);

				Hashtable transGroups = new Hashtable();

				foreach (XmlNode groupNode in transGroupNodes) {
					try {
						string name = groupNode.Attributes[NAME_ATTRIBUTE].Value;
						List<Transition> transitions = this.LoadTransitions(groupNode);
						transGroups.Add(name, transitions);
					} catch (Exception) {}
				}

				//======================================================
				//  load the config settings
				//======================================================

				//  read the config settings from the states file
				XmlNode configNode = stateSettings.SelectSingleNode("/states/config");
				if (configNode != null) {
					try {
						bool trace = bool.Parse(configNode.Attributes["trace"].Value);
						info.AddConfig("trace", trace);
					} catch (Exception) {}
				}

				//======================================================
				//  load the states
				//======================================================

				//  get the "state" nodes
				XmlNodeList stateNodes = stateSettings.SelectNodes("/states/state");

				IState state = null;
				NameValueCollection attributes = new NameValueCollection();

				foreach (XmlNode stateNode in stateNodes) {
					try {
						string type = stateNode.Attributes[TYPE_ATTRIBUTE].Value;
						long id = long.Parse(stateNode.Attributes[ID_ATTRIBUTE].Value);

						attributes.Clear();
						foreach (XmlAttribute attr in stateNode.Attributes) {
							attributes.Add(attr.Name, attr.Value);
						}

						state = StateFactory.MakeState(type, id, attributes);

						if (state == null) {
							throw new ApplicationException("Null state exception.");
						}
					} catch (Exception e) {
						LogManager.GetCurrentClassLogger().Error(
							errorMessage => errorMessage("Failed to make state: node = {0}", stateNode.OuterXml), e);
					}

					//  if we successfully generated the state...
//				if (state != null) {
					if (state is BaseState) {
						//  load the state's transitions
						List<Transition> transitions = this.LoadTransitions(stateNode);
						foreach (Transition trans in transitions) {
							try {
								((BaseState)state).AddTransition(trans);
							} catch (Exception e) {
								throw new ApplicationException(
									string.Format("Invalid transition '{0}' in state {1}.", trans.Name, state.Id),
									e);
							}
						}

						//  add the transitions from transition groups if there are any
						XmlNodeList groupNodes = stateNode.SelectNodes(TRANSITION_GROUP_NAME);
						foreach (XmlNode groupNode in groupNodes) {
							try {
								//  get the name of the tranistion group
								string name = groupNode.Attributes[NAME_ATTRIBUTE].Value;
								//  if no transition group is defined for the given name, throw exception
								if (!transGroups.ContainsKey(name)) {
									throw new ApplicationException(string.Format(
																	"No transition group defined for '{0}'. State = {1}.", name, state.Id));
								}
								//  add the transitions from the transition group to the state
								foreach (Transition trans in (List<Transition>)transGroups[name]) {
									((BaseState)state).AddTransition(trans.Clone(state.Id));
								}
							} catch (Exception e) {
								LogManager.GetCurrentClassLogger().Error(
									errorMessage => errorMessage("ConfigSmStateDao.LoadStates: failure loading transitiongroup (" + groupNode.OuterXml + "): "), e);
							}
						}
					}

					states.Add(state);
//				}
				}

				//  dump the local transition groups hashtable
				transGroups.Clear();

				LogManager.GetCurrentClassLogger().Debug(
					debugMessage => debugMessage("ConfigSmStateDao -  successfully loaded page config settings"));
			} catch (Exception e) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("ConfigSmStateDao.LoadStates: "), e);
				throw;
			}

			info.States = states.ToArray();
			return info;
		}

		#endregion


		/// <summary>
		/// Loads the transitions defined within the given node.
		/// </summary>
		/// <remarks>
		/// If the given parent node has an <b>id</b> attribute, that ID value is used
		/// as the parent state ID for the transitions loaded from the node.
		/// </remarks>
		/// <param name="parentNode">The XmlNode to load the transitions from.</param>
		/// <returns>A <b>List&lt;Transition&gt;</b> containing the transitions loaded from the given node.</returns>
		private List<Transition> LoadTransitions(
			XmlNode parentNode)
		{
			List<Transition> tranistions = new List<Transition>();

			//  load the node's transitions
			XmlNodeList transitionNodes = parentNode.SelectNodes("transition");

			long id = -1;
			//  if the parent node has an ID, get it
			if (parentNode.Attributes[ID_ATTRIBUTE] != null) {
				try {
					id = long.Parse(parentNode.Attributes[ID_ATTRIBUTE].Value);
				} catch (Exception) {}
			}

			foreach (XmlNode transNode in transitionNodes) {
				try {
					Transition trans = new Transition(id,
									long.Parse(transNode.Attributes["targetState"].Value),
									transNode.Attributes["name"].Value.ToLower(),
									(transNode.Attributes["publishKeyParams"] == null)
											? null
											: transNode.Attributes["publishKeyParams"].Value.ToLower(),
									(transNode.Attributes["contentProvider"] == null)
											? null
											: transNode.Attributes["contentProvider"].Value.ToLower());
					tranistions.Add(trans);
				} catch (Exception e) {
					LogManager.GetCurrentClassLogger().Error(
						errorMessage => errorMessage("ConfigSmStateDao.LoadTransitions: failure loading transition ({0}): ", transNode.OuterXml), e);
				}
			}

			return tranistions;
		}


		/// <summary>
		/// Gets the value of an attribute of a <b>XmlNode</b> if it exists.
		/// </summary>
		/// <remarks>
		/// This is simply a convenience method that performs the checks for the existance
		/// of the node and attribute before attempting to get the value.
		/// </remarks>
		/// <param name="node">The <b>XmlNode</b> to get the attribute from.</param>
		/// <param name="attribute">The name of the attribute to get the value for.</param>
		/// <returns>The value of the specified attribute of the given <b>XmlNode</b>, or
		///		null if the node or attribute do not exist.</returns>
		private string GetAttributeValue(
			XmlNode node,
			string attribute)
		{
			string val = null;

			if ((node != null) && (node.Attributes[attribute] != null)) {
				val = node.Attributes[attribute].Value;
			}

			return val;
		}
	}
}