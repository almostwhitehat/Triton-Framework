using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Common.Logging;
using Triton.Controller.StateMachine;

namespace Triton.Graphml.Model.Dao
{

	#region History

	// History:
	// 08/16/2009	GV	Ran clean up and reformat.
	// 09/27/2009

	#endregion

	/// <summary>
	/// <b>GraphmlSmStateDao</b> implements the <b>IStateMachineStatesDao</b> 
	/// interface to load state/transition information from an GraphML file.
	/// </summary>
	///	<author>Matthew Cummings</author>
	///	<created>02/27/08</created>
	public class GraphmlSmStateDao : IStateMachineStatesDao
	{
		private const string ACTION_ATTRIBUTE = "action";
		private const string ID_ATTRIBUTE = "id";
		private const string NAME_ATTRIBUTE = "name";
		private const string PAGE_ATTRIBUTE = "page";
		private const string STOP_ATTRIBUTE = "stop";

		/// <summary> 
		/// private logger refference
		/// </summary>
		private readonly ILog log = LogManager.GetCurrentClassLogger();

		#region IStateMachineStatesDao Members

		/// <summary>
		/// Loads the state/tranistion information for an XML config file.
		/// </summary>
		/// <remarks>
		/// The path to the config file to load is specified in the application's config file
		/// appSettings section, with the "statesConfigPath" setting.
		/// </remarks>
		/// <returns>A <b></b> object with the state/transition information loaded 
		/// from the config file.</returns>
		public StateInfo LoadStates()
		{
			StateInfo info = new StateInfo();
			Hashtable transGroups = new Hashtable();

			try {
				// get the path to the states.config directory from web.config
				string statesConfigPath = ConfigurationManager.AppSettings["rootPath"]
				                          + ConfigurationManager.AppSettings["statesDiagramPath"];

				if (statesConfigPath == null) {
					throw new ApplicationException("No states.config path in config file.");
				}

				// read all the .graphml files in the directory.
				// XmlDocument stateSettings = new XmlDocument();

				// Key is the StateID, value is the element representing the state
				Dictionary<long, IState> states = new Dictionary<long, IState>();


				// key is the id of the node, value is the transition name
				Dictionary<string, Dictionary<string, long>> transitionGroups = new Dictionary<string, Dictionary<string, long>>();
				Dictionary<long, List<string>> stateTransitionGroups = new Dictionary<long, List<string>>();

				if (Directory.Exists(statesConfigPath)) {
					if (Directory.GetFiles(statesConfigPath, "*.graphml", SearchOption.TopDirectoryOnly).Length > 0) {
						foreach (string fileName in Directory.GetFiles(statesConfigPath, "*.graphml", SearchOption.TopDirectoryOnly)) {
							this.ProcessGraphmlFile(fileName, states, transitionGroups, stateTransitionGroups);
						}
						foreach (KeyValuePair<long, List<string>> kvp in stateTransitionGroups) {
							long fromStateId = kvp.Key;
							IState state = states[fromStateId];
							foreach (string stateTransitionGroup in kvp.Value) {
								if (transitionGroups.ContainsKey(stateTransitionGroup)) {
									Dictionary<string, long> transitionGroup = transitionGroups[stateTransitionGroup];
									foreach (KeyValuePair<string, long> trans in transitionGroup) {
										long toStateId = trans.Value;
										Transition transition = new Transition(fromStateId, toStateId, trans.Key, null);
										try {
											((BaseState)state).AddTransition(transition);
										} catch {
											this.log.Debug(debug => debug("Attempting to add a transition ({0}) to a state ({1}) that already exists.",
											                              trans.Key,
											                              toStateId));
										}
									}
								} else {
									this.log.Debug(debug => debug("Unable to locate transition group {0} for stateid {1}", stateTransitionGroup, fromStateId));
								}
							}
						}
					} else {
						this.log.Error(error => error("LoadStates: No States config files found"));
						throw new IndexOutOfRangeException("LoadStates: No States config files found");
					}
				} else {
					this.log.Error(error => error("LoadStates: No States Folder found"));
					throw new NotSupportedException("LoadStates: No States Folder found");
				}

				info.States = states.Values.ToArray();

				this.log.Info(infoMessage => infoMessage("Successfully loaded page config settings"));
			} catch (Exception e) {
				this.log.Error(error => error("Error occured in LoadStates: ", e));
				throw;
			} finally {
				transGroups.Clear();
			}

			return info;
		}

		#endregion

		private void ProcessGraphmlFile(
			string filename,
			IDictionary<long, IState> states,
			IDictionary<string, Dictionary<string, long>> transitionGroups,
			IDictionary<long, List<string>> stateTransitionGroups)
		{
			// Key is the id of the node, Value is the stateID
			Dictionary<string, int> transitionLookup = new Dictionary<string, int>();

			Dictionary<string, string> transitionGroupLookup = new Dictionary<string, string>();


			XDocument xdoc = XDocument.Load(filename);
			XPathNavigator nav = xdoc.CreateNavigator();

			IEnumerable<XPathNavigator> nodes = this.FindNodes(nav, "node");
			IEnumerable<XPathNavigator> edges = this.FindNodes(nav, "edge");

			foreach (XPathNavigator node in nodes) {
				string type = string.Empty;
				string contents = string.Empty;
				string nodeId = string.Empty;

				XPathNavigator nodeAttributes = node.Clone();
				nodeAttributes.MoveToFirstAttribute();
				do {
					if (nodeAttributes.Name == "id") {
						nodeId = nodeAttributes.Value;
					}
				} while (nodeAttributes.MoveToNextAttribute());

				node.MoveToChild(XPathNodeType.All);

				XPathNavigator label = null;
				XPathNavigator shape = null;

				do {
					if ((node.Name == "data") && node.HasAttributes) {
						XPathNavigator dataNodes = node.Clone();
						do {
							XPathNavigator dataNode = dataNodes.Clone();
							string key = dataNodes.GetAttribute("key", string.Empty);
							if (!string.IsNullOrEmpty(key) &&
							    key.StartsWith("d") &&
							    dataNode.HasChildren) {
								try {
									XPathNavigator shapeNode = dataNode.Clone();
									if (shapeNode.HasChildren) {
										shapeNode.MoveToChild(XPathNodeType.All);
										shapeNode.MoveToChild(XPathNodeType.All);
										do {
											switch (shapeNode.Name) {
												case "y:Fill":
													break;
												case "y:NodeLabel":
													label = shapeNode.Clone();
													break;
												case "y:BorderStyle":
													break;
												case "y:Shape":
													shape = shapeNode.Clone();
													break;
											}
										} while (shapeNode.MoveToNext());

										Trace.WriteLine(string.Empty);
										if (shape != null) {
											shape.MoveToFirstAttribute();
										}
										if (shape != null) {
											type = shape.Value;
										}
										if (label != null) {
											contents = label.Value;
										}
									}
								} catch (NullReferenceException exception) {
									this.log.Error(
										errorMessage =>
										errorMessage("ProcessFile() and error occured processing file ({1}) {0}", exception, filename));
								}
							}
						} while (dataNodes.MoveToNext());
					}
				} while (node.MoveToNext());
				try {
					int stateId;
					switch (type) {
						case "rectangle":
							IState pageNode = this.CreatePageState(out stateId, contents);
							if (states.ContainsKey(pageNode.Id)) {
								throw new ApplicationException(string.Format("Page State ({0})already exists.", pageNode.Id));
							}

							states[pageNode.Id] = pageNode;
							transitionLookup[nodeId] = stateId;
							break;
						case "octagon":
							IState stopNode = this.CreateStopState(out stateId, contents);
							if (states.ContainsKey(stopNode.Id)) {
								throw new ApplicationException(string.Format("Stop State ({0})already exists.", stopNode.Id));
							}

							states[stopNode.Id] = stopNode;
							transitionLookup[nodeId] = stateId;
							break;
						case "roundrectangle":
							IState actionNode = this.CreateActionState(out stateId, contents);
							if (states.ContainsKey(actionNode.Id)) {
								throw new ApplicationException(string.Format("Action State ({0})already exists.", actionNode.Id));
							}

							states[actionNode.Id] = actionNode;
							transitionLookup[nodeId] = stateId;
							break;
						case "ellipse":
							transitionLookup[nodeId] = int.Parse(contents);
							break;
						case "diamond":
							if (transitionGroupLookup.ContainsKey(nodeId)) {
								throw new ApplicationException("Transition Group Definition already exists.");
							}

							transitionGroupLookup[nodeId] = contents;
							break;
						case "parallelogram":
							string transitionGroupName = string.Empty;
							string[] split = contents.Split('\n');
							NameValueCollection nodeContents = this.ProcessContent(split);
							if (nodeContents[NAME_ATTRIBUTE] == null) {
								transitionGroupName = split[0];
							}

							Dictionary<string, long> tranGroup = new Dictionary<string, long>();
							try {
								transitionGroupLookup[nodeId] = transitionGroupName;
								transitionGroupLookup[transitionGroupName] = nodeId;
								tranGroup = transitionGroups[transitionGroupName];
							} catch {
								transitionGroups.Add(transitionGroupName, tranGroup);
							}

							break;
						default:
							break;
					}
				} catch (ApplicationException ex) {
					this.log.Warn(warn => warn("The was an error processing node {0} : {1}.", nodeId, ex.ToString()));
				} catch (Exception ex) {
					throw new ApplicationException("A fatal error has occured. Could not load states.", ex);
				}
			}

			// process all the edges
			foreach (XPathNavigator edge in edges) {
				string transitions = string.Empty;
				string fromNodeId = string.Empty;
				string toNodeId = string.Empty;

				XPathNavigator graphAttr = edge.Clone();
				graphAttr.MoveToFirstAttribute();
				do {
					switch (graphAttr.Name) {
						case "source":
							fromNodeId = graphAttr.Value;
							break;
						case "target":
							toNodeId = graphAttr.Value;
							break;
					}
				} while (graphAttr.MoveToNextAttribute());

				// find the y:PolyLineEdge node
				XPathNavigator dataNodes = edge.Clone();
				dataNodes.MoveToChild(XPathNodeType.All);


				do {
					if ((dataNodes.Name == "data") && dataNodes.HasAttributes && dataNodes.HasChildren) {
						string key = dataNodes.GetAttribute("key", string.Empty);

						if (!string.IsNullOrEmpty(key) && key.StartsWith("d")) {
							XPathNavigator dataNode = dataNodes.Clone();

							dataNode.MoveToChild(XPathNodeType.All);
							dataNode.MoveToChild(XPathNodeType.All);

							do {
								switch (dataNode.Name) {
									case "y:EdgeLabel":
										// get the attributes
										transitions += edge.Value;
										break;
								}
							} while (dataNode.MoveToNext());
						}
					}
				} while (dataNodes.MoveToNext());

				// process the info
				int fromStateId = int.MinValue;
				if (transitionLookup.ContainsKey(fromNodeId)) {
					fromStateId = transitionLookup[fromNodeId];
				} else {
					this.log.Error(error => error("Unable to locate transition node from node id {0}.", fromNodeId));
				}
				if (transitionGroupLookup.ContainsKey(toNodeId)) {
					string[] edgecontents = transitionGroupLookup[toNodeId].Split('\n');
					string[] transitionSplit = transitions.Split('\n');

					foreach (string transition in transitionSplit) {
						if (transition == "transitionGroup") {
							// add this to a list of transition for a state
							// to be filled later
							//NameValueCollection content = this.ProcessContent(edgecontents);
							foreach (string key in edgecontents) {
								List<string> transitionGroupList = new List<string>();
								try {
									transitionGroupList = stateTransitionGroups[fromStateId];
								} catch {
									if (!stateTransitionGroups.ContainsKey(fromStateId)) {
										stateTransitionGroups.Add(fromStateId, transitionGroupList);
									}
								}
								if (!transitionGroupList.Contains(key)) {
									transitionGroupList.Add(key);
								}
							}
						}
					}

					// this must be a transitionGroups transition
				} else if (transitionGroupLookup.ContainsKey(fromNodeId)) {
					string transitionGroupName = transitionGroupLookup[fromNodeId];
					//string transitionGroupNodeId = transitionGroupLookup[transitionGroupName];
					string[] transitionSplit = transitions.ToLower().Split('\n');

					foreach (string transition in transitionSplit) {
						if (transitionGroups.ContainsKey(transitionGroupName)) {
							Dictionary<string, long> transitionGroup = transitionGroups[transitionGroupName];

							// add this transition to the transitionGroup
							int toStateId = transitionLookup[toNodeId];
							if (!transitionGroup.ContainsKey(transition)) {
								transitionGroup.Add(transition, Convert.ToInt64(toStateId));
							} else {
								this.log.Info(info => info("Transition '{0}' already exists for TransitionGroup '{1}'", transition, transitionGroupName));
							}
						} else {
							// need to log something
							this.log.Info(info => info("Cannot locate transition group '{0}' while processing Edge", transitionGroupName));
						}
					}
				} else {
					if (transitionLookup.ContainsKey(toNodeId)) {
						int toStateId = transitionLookup[toNodeId];

						string[] transitionSplit = transitions.ToLower().Split('\n');

						// Temporary, finish then see if this was the best way to do this before merging into the core proper.
						List<string> contentProviderCheck = transitionSplit.ToList();
						string contentProvider = contentProviderCheck.Find(x => x.Contains("contentprovider:"));
						string providerType = null;

						if (!string.IsNullOrEmpty(contentProvider)) {
							providerType = contentProvider.Replace("contentprovider:", "");
							providerType = providerType.Trim();
							this.log.Info(info => info("Created transition with a content provider."));
							contentProviderCheck.Remove(contentProvider);
						}

						foreach (string transition in contentProviderCheck) {
							if (fromStateId > 0 && toStateId > 0) {
								try {
									Transition trans = new Transition(fromStateId, toStateId, transition, null, providerType);
									((BaseState)states[fromStateId]).AddTransition(trans);
								} catch (Exception) {
									this.log.Error(error => error("Error Adding Transition to State ({0} -> {1} -> ({2}))", fromStateId, transition, toStateId));
								}
							}
						}
					} else {
						this.log.Info(info => info("Unable to locate transition to node {0} in transition {1}", toNodeId, transitions));
					}
				}
			}

			nav.MoveToChild(XPathNodeType.All);
			nav.MoveToChild(XPathNodeType.All);
		}


		private IState CreateStopState(
			out int stateId,
			string contents)
		{
			string[] split = contents.Split('\n');

			if (split.Length < 2) {
				throw new ApplicationException("Unable to create Stop state. not enough information provided. an Id and a name is required.");
			}

			NameValueCollection attributes = this.ProcessContent(split);

			if (attributes[ID_ATTRIBUTE] == null) {
				stateId = int.Parse(split[0].Trim());
				attributes[ID_ATTRIBUTE] = split[0].Trim();
			} else {
				stateId = int.Parse(attributes[ID_ATTRIBUTE].Trim());
			}

			if (string.IsNullOrEmpty(attributes[NAME_ATTRIBUTE]) && (split.Length >= 1)) {
				attributes[NAME_ATTRIBUTE] = split[1].Trim();
			}

			IState state = StateFactory.MakeState(STOP_ATTRIBUTE, stateId, attributes);

			return state;
		}


		/// <summary> this method creates a page state from the contents of the page state node in the graphml file.
		/// </summary>
		/// <param name="stateId">
		/// The state id.
		/// </param>
		/// <param name="contents">
		/// The contents.
		/// </param>
		/// <returns> the newly created page state
		/// </returns>
		/// <exception cref="ApplicationException"> thrown when the wethod cannot 
		/// complete the task of creating a page state
		/// </exception>
		private IState CreatePageState(
			out int stateId,
			string contents)
		{
			string[] split = contents.Split('\n');

			if (split.Length < 2) {
				throw new ApplicationException("Cannot locate enough information to to create a page state");
			}

			NameValueCollection attributes = this.ProcessContent(split);

			if (string.IsNullOrEmpty(attributes[ID_ATTRIBUTE])) {
				stateId = int.Parse(split[0]);
				attributes[ID_ATTRIBUTE] = split[0];
			} else {
				stateId = int.Parse(attributes[ID_ATTRIBUTE]);
			}

			if (string.IsNullOrEmpty(attributes[NAME_ATTRIBUTE]) && (split.Length >= 1)) {
				attributes[NAME_ATTRIBUTE] = split[1];
			}

			if (string.IsNullOrEmpty(attributes[PAGE_ATTRIBUTE]) && (split.Length >= 1)) {
				attributes[PAGE_ATTRIBUTE] = split[1];
			}

			IState state = StateFactory.MakeState(PAGE_ATTRIBUTE, stateId, attributes);

			return state;
		}


		private IState CreateActionState(
			out int stateId,
			string contents)
		{
			string[] split = contents.Split('\n');

			if (split.Length < 2) {
				throw new ApplicationException("Unable to create Action State. Id, Action are required.");
			}

			NameValueCollection attributes = this.ProcessContent(split);

			if (string.IsNullOrEmpty(attributes[ID_ATTRIBUTE])) {
				stateId = int.Parse(split[0].Trim());
				attributes[ID_ATTRIBUTE] = split[0].Trim();
			} else {
				stateId = int.Parse(attributes[ID_ATTRIBUTE].Trim());
			}

			int id = stateId;

			this.log.Debug(debug => debug("Starting to process state id {0}", id));

			if (string.IsNullOrEmpty(attributes[NAME_ATTRIBUTE]) && (split.Length >= 1)) {
				attributes[NAME_ATTRIBUTE] = split[1].Trim();
			}

			if (string.IsNullOrEmpty(attributes[ACTION_ATTRIBUTE]) && (split.Length >= 1)) {
				this.log.Debug(debug => debug("Defaulting the action attribute to {0}", split[1]));
				attributes[ACTION_ATTRIBUTE] = split[1].Trim();
			}


			IState state = StateFactory.MakeState(ACTION_ATTRIBUTE, stateId, attributes);

			return state;
		}


		/// <summary>
		/// </summary>
		/// <param name="contents">
		/// The contents.
		/// </param>
		/// <returns>
		/// </returns>
		private NameValueCollection ProcessContent(string[] contents)
		{
			NameValueCollection keyVal = new NameValueCollection();

			foreach (string content in contents) {
				try {
					string newcontent = content;

					// remove the brackets
					if (content.StartsWith("[")) {
						newcontent = content.Remove(0, 1);
						newcontent = newcontent.Remove(newcontent.Length - 1, 1);
					}

					if ((newcontent.Trim().Length > 0) && newcontent.Contains(':')) {
						string[] split = newcontent.Split(':');

						keyVal[split[0].Trim()] = split[1].Trim();
					} else {
						this.log.Warn(warn => warn("Every line of a state definition must have a single value or colon seperated pair."));
					}
				} catch (Exception ex) {
					// suppress all errors
					// this.logger.Error("process Content", ex);
					this.log.Warn(warn => warn("Error occured in ProcessContent.", ex));
				}
			}

			return keyVal;
		}


		private IEnumerable<XPathNavigator> FindNodes(
			XPathNavigator doc,
			string name)
		{
			List<XPathNavigator> nodes = new List<XPathNavigator>();

			if (doc.Name == name) {
				nodes.Add(doc.Clone());
			}

			if (doc.HasChildren) {
				doc.MoveToChild(XPathNodeType.All);
				do {
					XPathNavigator child = doc.Clone();
					nodes.AddRange(this.FindNodes(child, name));
				} while (doc.MoveToNext());
			}


			return nodes;
		}
	}
}