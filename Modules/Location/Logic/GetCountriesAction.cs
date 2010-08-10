using System;
using System.Collections.Generic;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Location.Model;
using Triton.Location.Model.Dao;
using Triton.Location.Support.Request;
using Triton.Logic.Support;
using Triton.Model;
using Triton.Model.Dao;

namespace Triton.Location.Logic
{
	/// <summary>
	/// Returns a SearchResult of type Country into the request.
	/// </summary>
	public class GetCountriesAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Request item name for SearchResult Country output.
		/// </summary>
		public string CountryItemNameOut{ get; set; }

		/// <summary>
		/// Default constructor for GetCountriesAction.
		/// </summary>
		public GetCountriesAction()
		{
			this.CountryItemNameOut = ItemNames.Country.DEFAULT_SEARCH_RESULT_COUNTRIES;
		}

		#region IAction Members

		public string Execute(TransitionContext context)
		{
			string retEvent = Events.Error;

			try {

				// For now this just returns all the countries, will add filtering as need be.
				List<Country> countries = new List<Country>(DaoFactory.GetDao<ICountryDao>().Get(new Country()));

                SearchResult<Country> results = new SearchResult<Country>(countries.ToArray());

				retEvent = EventUtilities.GetSearchResultEventName(results.Items.Length);

				context.Request.Items[this.CountryItemNameOut] = results;
				
			} catch(Exception ex) {
				logger.Error(
					error => error("Error in GetCountriesAction -> Execute : "), ex);
			}

			return retEvent;
		}

		#endregion

		#region Nested type: Events

		/// <summary>
		/// Event names to return to the controller.
		/// </summary>
		public class Events
		{
			public static string Zero
			{
				get { return EventNames.ZERO; }
			}

			public static string One
			{
				get { return EventNames.ONE; }
			}

			public static string Multiple
			{
				get { return EventNames.MULTIPLE; }
			}

			public static string Error
			{
				get { return EventNames.ERROR; }
			}
		}

		#endregion
	}
}
