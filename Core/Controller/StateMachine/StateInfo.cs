using System.Collections;
using System.Collections.Specialized;

namespace Triton.Controller.StateMachine
{
	/// <summary>
	/// Transfer object to return state/transition data and StateManager configuration settings
	/// from the DAO.
	/// </summary>
	public class StateInfo
	{
		private readonly Hashtable configSettings = CollectionsUtil.CreateCaseInsensitiveHashtable();


		/// <summary>
		/// Constructs an empty <b>StateInfo</b>.
		/// </summary>
		public StateInfo()
		{
		}


		/// <summary>
		/// Constructs a <b>StateInfo</b> with the given configuration settings 
		/// and states collection.
		/// </summary>
		/// <param name="configSettings">The configuration settings.</param>
		/// <param name="states">The collection of states.</param>
		public StateInfo(
			Hashtable configSettings,
			IState[] states)
		{
			this.configSettings = configSettings;
			this.States = states;
		}


		/// <summary>
		/// Gets or sets the collection of <b>IState</b>s.
		/// </summary>
		public IState[] States
		{
			get;
			set;
		}


		/// <summary>
		/// Gets the configruation settings.
		/// </summary>
		public Hashtable Config
		{
			get {
				return this.configSettings;
			}
		}


		/// <summary>
		/// Adds a new configuration setting to the config settings.
		/// </summary>
		/// <param name="name">The name of the new config setting.</param>
		/// <param name="value">The value of the new config setting.</param>
		public void AddConfig(
			string name,
			object value)
		{
			this.configSettings.Add(name, value);
		}
	}
}