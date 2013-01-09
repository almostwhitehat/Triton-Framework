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
    /// 
    /// </summary>
    public class Countries : SingletonBase<Countries, Country, string>
    {
        private List<Country> countries;
        private static Countries instance = null;
        private static object syncLock = new object();

        public static Countries Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncLock)
                    {
                        if (instance == null)
                        {
                            instance = new Countries();
                        }
                    }
                }

                return instance;
            }
        }

        public Countries()
        {
            LoadCountries();
        }

        /// <summary>
        /// Indexer to get Country by ID
        /// </summary>
        /// <param name="id"></param>
        public Country this[int id]
        {
            get { return countries.FirstOrDefault(c => c.Id == id); }
        }

        public List<Country> Items {get { return countries; }} 

        /// <summary>
        /// Loads the states from the database and populates singleton
        /// </summary>
        private void LoadCountries()
        {
            ICountryDao dao = DaoFactory.GetDao<ICountryDao>();
            List<Country> loadResults = new List<Country>(dao.Get(new Country()));

            if(loadResults != null && loadResults.Count > 0)
            {
                countries = loadResults;
            }
        }

    }
}
