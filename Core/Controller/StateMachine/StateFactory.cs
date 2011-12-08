using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using Triton.Utilities.Reflection;

namespace Triton.Controller.StateMachine
{

	#region History

	// History:
	//  11/21/11	SD	Updated handle StatePrerequisites.

	#endregion

	/// <summary>
	/// Factory class to instantiate State objects.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class StateFactory
	{
		private const string	NAME_ATTRIBUTE = "name";
		private const string	STATES_CONFIG_PATH = "controllerSettings/states";
		private const char		PREREQUISITE_VALUES_DELIMITOR = '|';
		private const char		PREREQUISITE_PARTS_DELIMITOR = ',';
		private const int		PREREQUISITE_PART_STATE_ID = 0;
		private const int		PREREQUISITE_PART_STATE_EVENT = 1;
		//private static Hashtable	typeMappings	= null;
		//private static bool			mappingsLoaded	= false;


		/// <summary>
		/// Creates a new State instance for the specified type.
		/// </summary>
		/// <remarks>
		/// State prerequisite are included in the <c>attributes</c> collection with the
		/// attribute name of "prerequisite" and a value in the form of
		/// [{name}=]{start_state_id},{start_event}|[{name}=]{start_state_id},{start_event}|...
		/// where {name} is the name of the (optional) name of the prerequisite, {start_state_id}
		/// is the ID of the start state for the prerequisite flow and {start_event} is the
		/// event from the start state to follow. {name} is optional. With no name the value
		/// would look like: {start_state_id},{start_event}|{start_state_id},{start_event}|...
		/// </remarks>
		/// <param name="stateType">The type of State to make.</param>
		/// <param name="id">The state ID for the state.</param>
		/// <param name="attributes">A <b>NameValueCollection</b> containing the attributes
		///			for the state.</param>
		/// <returns>A new instance of a State of the specified type.</returns>
		public static IState MakeState(
			string stateType,
			long id,
			NameValueCollection attributes)
		{
			StatesConfigHandler.StatesConfig statesConfig =
					(StatesConfigHandler.StatesConfig)ConfigurationManager.GetSection(STATES_CONFIG_PATH);

			string typeName = (string)statesConfig.StateTypes[stateType];

			Type type = Type.GetType(typeName);

			string stateName = "";
			if (!string.IsNullOrEmpty(attributes[NAME_ATTRIBUTE])) {
				stateName = attributes[NAME_ATTRIBUTE];
			}

					//  try to create a state with a constructor that defines id, and name, 
					//  if thats missing try just with id.
			IState state;
			if (type.GetConstructor(new [] {typeof (long), typeof (string)}) != null) {
				state = (IState)Activator.CreateInstance(type, id, stateName);
			} else if (type.GetConstructor(new [] {typeof (long)}) != null) {
				state = (IState)Activator.CreateInstance(type, id);
			} else {
				throw new MissingMethodException(string.Format("Could not find the constructor nessesary to create the state {0} and type of {1}. The constructor needs to either accept an Id and Name parameters, or just Id.", id, stateType));
			}

			//  state prerequisites   prerequisite=startState1,event1|startState2,event2|...


			if ((state != null) && (attributes != null)) {
				ReflectionUtilities.Deserialize(state, attributes);

						//  determine if the state has an AddAttribute method
				bool hasAddAttribute = ReflectionUtilities.HasMethod(state, "AddAttribute", "".GetType(), "".GetType());
						//  determine if the state has a Prerequisite property
				bool hasPrerequisite = ReflectionUtilities.HasProperty(state, "Prerequisite");//, typeof(StatePrerequisite));

				foreach (string attrName in attributes.AllKeys) {
					if ("prerequisite".Equals(attrName.ToLower()) && hasPrerequisite) {
								//  format of the prerequisite attribute value is: {name}={start_state_id},{start_event}|...
						List<StatePrerequisite> prerequisites = new List<StatePrerequisite>();
						string[] prerequisiteInfo = attributes[attrName].Split(PREREQUISITE_VALUES_DELIMITOR);
						foreach (string prereqRec in prerequisiteInfo) {
									//  split the name from the stateID/event
							string[] prereqNameVal = prereqRec.Split('=');
									//  for the prerequisite name, if provided
							string prereqName = null;
									//  default the prerequisite definition to the value from the initial split (assume no name)
							string prereqDef = prereqRec;
									//  if there are 2 peices from the "=" split, a name was given
							if (prereqNameVal.Length > 1) {
										//  get the name
								prereqName = prereqNameVal[0];
										//  set the definition to the value after the name
								prereqDef = prereqNameVal[1];
							}
									//  split the definition to get the start state ID and event
							string[] prereqVals = prereqDef.Split(PREREQUISITE_PARTS_DELIMITOR);
							long startStateId;
							if (long.TryParse(prereqVals[PREREQUISITE_PART_STATE_ID], out startStateId)) {
								prerequisites.Add(new StatePrerequisite {
										Name = prereqName,
										StartStateId = startStateId,
										StartEvent = prereqVals[PREREQUISITE_PART_STATE_EVENT] });
							} else {
							}
						}

						ReflectionUtilities.SetPropertyValue(state, "Prerequisite", prerequisites.ToArray());

							//  if the state has an "AddAttribute", add the attribute to the 
							//  general attribute collection
					} else if (hasAddAttribute) {
						ReflectionUtilities.CallMethod(state, "AddAttribute", attrName, attributes[attrName]);
					}
				}
			}

			return state;
		}
	}
}