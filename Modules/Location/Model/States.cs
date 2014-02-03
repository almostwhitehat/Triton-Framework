using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Triton.Location.Model.Dao;
using Triton.Model;
using Triton.Model.Dao;

namespace Triton.Location.Model
{
    /// <summary>
    /// Singleton for storing location states for use in the deserialize address
    /// </summary>
    public class States : SingletonBase<States, State, string>
    {
        private static States instance = null;
        
        private static object syncLock = new object();

        private List<State> states;

        public static States Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncLock)
                    {
                        if (instance == null)
                        {
                            instance = new States();
                        }
                    }
                }

                return instance;
            }
        }

        public States()
        {
            LoadStates();
        }

        /// <summary>
        /// Indexer to get a State by ID
        /// </summary>
        /// <param name="id">The ID of the state to get</param>
        /// <returns>The State with the given ID</returns>
        public State this[int id]
        {
            get { return states.FirstOrDefault(s => s.Id.Value == id); }
        }

	/// <summary>
        /// Indexer to get a State by Code
        /// </summary>
        /// <param name="code">The ID of the state to get</param>
        /// <returns>The State with the given Code</returns>
        public State this[string code]
        {
            get { return states.FirstOrDefault(s => s.Code == code); }
        }


        /// <summary>
        /// Loads the states from the database and populates singleton
        /// </summary>
        private void LoadStates()
        {
            IStateDao dao = DaoFactory.GetDao<IStateDao>();
            List<State> loadResults = new List<State>(dao.Get(new State
            {
                IsTerritory = false
            }));

            if(loadResults != null && loadResults.Count > 0)
            {
                states = loadResults;
            }
        }
    }
}
