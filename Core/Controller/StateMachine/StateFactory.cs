using System;
using System.Collections.Specialized;
using System.Configuration;
using Triton.Utilities.Reflection;

namespace Triton.Controller.StateMachine
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// Factory class to instantiate State objects.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class StateFactory
	{
		private const string NAME_ATTRIBUTE = "name";
		private const string STATES_CONFIG_PATH = "controllerSettings/states";
		//private static Hashtable	typeMappings	= null;
		//private static bool			mappingsLoaded	= false;


		/// <summary>
		/// Creates a new State instance for the specified type.
		/// </summary>
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

			//try to create a state with a constructor that defines id, and name, 
			//if thats missing try just with id.
			IState state;
			if (type.GetConstructor(new [] {typeof (long), typeof (string)}) != null) {
				state = (IState)Activator.CreateInstance(type, id, stateName);
			} else if (type.GetConstructor(new [] {typeof (long)}) != null) {
				state = (IState)Activator.CreateInstance(type, id);
			} else {
				throw new MissingMethodException(string.Format("Could not find the constructor nessesary to create the state {0} and type of {1}. The constructor needs to either accept an Id and Name parameters, or just Id.", id, stateType));
			}

			if ((state != null) && (attributes != null)) {
				ReflectionUtilities.Deserialize(state, attributes);

				//  if the state has an "AddAttribute", add the attribute to the 
				//  general attribute collection
				if (ReflectionUtilities.HasMethod(state, "AddAttribute", "".GetType(), "".GetType())) {
					foreach (string attrName in attributes.AllKeys) {
						ReflectionUtilities.CallMethod(state, "AddAttribute", attrName, attributes[attrName]);
					}
				}
			}

			return state;
		}
	}
}