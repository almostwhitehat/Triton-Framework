using System;
using System.Collections;
using System.Configuration;
using Common.Logging;
using Triton.Configuration;

namespace Triton.Controller.StateMachine
{

	#region History

	// History:
	// 6/2/2009		KP  Changed the logging to Common.logging.
	// 09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method

	#endregion

	/// <summary>
	/// <c>StateManager</c> is a singleton responsible for managing
	/// the flow of the system.
	/// </summary>
	///	<author>Scott Dyke</author>
	public sealed class StateManager
	{
		private const string ACTION_ATTRIBUTE = "action";
		private const string EXCLUDE_PARAMS_ATTRIBUTE = "publishKeyExcludeParams";
		private const string ID_ATTRIBUTE = "id";
		private const string NAME_ATTRIBUTE = "name";
		private const string PAGE_ATTRIBUTE = "page";
		private const string PUBLISH_ATTRIBUTE = "publish";
		private const string SECTION_ATTRIBUTE = "section";
		private const string SITE_ATTRIBUTE = "site";
		/// <summary>
		/// The config section name of the settings for states in web.config
		/// </summary>
		private const string SECTION_NAME = "states";
		private const string STATES_CONNECTION = "connection";
		private const string STATES_DAO = "dao";
		private const string STATES_SOURCE = "source";
		private const string STATUS_LOG = "Singletons";
		private static readonly object syncRoot = new object();
		private static StateManager instance;

		private Hashtable idHash;
		private bool loading;

		// Explicit static constructor to tell C# compiler
		// not to mark type as beforefieldinit


		private StateManager()
		{
			LogManager.GetCurrentClassLogger().Info(
				infoMessage => infoMessage("StateManager - starting load."));

			this.LoadStates();

			LogManager.GetCurrentClassLogger().Info(
				infoMessage => infoMessage("StateManager - load completed."));
		}


		/// <summary>
		/// Returns the single instance of the <c>StateManager</c>.
		/// </summary>
		/// <returns>The single instance of the <c>StateManager</c></returns>
		public static StateManager GetInstance()
		{
			if (instance == null) {
				//  ensure only one thread is able to instantiate an instance at a time.
				lock (syncRoot) {
					if (instance == null) {
						instance = new StateManager();
					}
				}
			}

			return instance;
		}


		/// <summary>
		/// Gets a state from the state map for the given ID.
		/// </summary>
		/// <param name="id">The ID of the state to get.</param>
		/// <returns>The <c>State</c> with the specified ID, or null if no such state.</returns>
		public IState GetState(
			long id)
		{
			IState state = (IState) this.idHash[id];

			return state;
		}


		/// <summary>
		/// Returns an instance of a <c>IStateMachineStatesDao</c>.  The concrete class
		/// instantiated is defined in the config file.
		/// </summary>
		/// <returns>A <c>IPublishDao</c>.</returns>
		private static IStateMachineStatesDao GetSmStatesDao()
		{
			// TODO: change to get class from "publishing" section
			//		NameValueCollection config = (NameValueCollection)System.Configuration.ConfigurationSettings.GetConfig("daoSettings/IPublishDao");
			//  TODO: check for name from config file -- throw exception if no name?
			//		string daoClass = config["class"];
//		NameValueCollection config = ConfigurationManager.GetSection(
//				"triton/states") as NameValueCollection;
//		string daoClass = config[STATES_DAO];
//		string connType = config[STATES_CONNECTION];
			StatesConfigHandler.StatesConfig statesConfig =
					(StatesConfigHandler.StatesConfig)ConfigurationManager.GetSection(TritonConfigurationSection.SectionName + "/" + SECTION_NAME);

			IStateMachineStatesDao dao = null;
			if (statesConfig.DaoConnectionName == null) {
				dao = (IStateMachineStatesDao) Activator.CreateInstance(Type.GetType(statesConfig.DaoType));
			} else {
				dao =
					(IStateMachineStatesDao)
					Activator.CreateInstance(Type.GetType(statesConfig.DaoType), new object[] {statesConfig.DaoConnectionName});
			}

			return dao;
		}


		/// <summary>
		/// Loads the state transition information from the database.
		/// </summary>
		private void LoadStates()
		{
			try {
				if (!this.loading) {
					this.loading = true;
					//  get the DAO
					IStateMachineStatesDao dao = GetSmStatesDao();

					//  get the states data from the source
					StateInfo info = dao.LoadStates();
					
					IState[] states = info.States;

					//  create & populate the hashtables
					this.idHash = Hashtable.Synchronized(new Hashtable());
					foreach (IState state in states) {
						this.idHash[state.Id] = state;
					}
				}
			} catch (Exception e) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("StateManager.LoadStatesFromDatabase: "), e);
				throw;
			} finally {
				this.loading = false;
			}
		}


		public void Reset()
		{
			instance = null;
			LogManager.GetCurrentClassLogger().Info(
					infoMessage => infoMessage("StateManager reset."));
		}
	}
}