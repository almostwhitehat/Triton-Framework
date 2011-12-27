using System;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Logic.Support;
using Triton.Location.Model;
using Triton.Location.Model.Dao;
using Triton.Location.Model.Dao.Support;
using Triton.Location.Support.Request;
using Triton.Model;
using Triton.Model.Dao;

namespace Triton.Location.Logic
{
	public class GetPersistedAddressAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();


		public GetPersistedAddressAction()
		{
			this.PersistedAddressItemNameOut = ItemNames.PersistedAddress.DEFAULT_SEARCH_RESULT_PERSISTED_ADDRESS;
		}

		
		public string PersistedAddressItemNameOut { get; set; }

		#region IAction Members


		public string Execute(TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				IPersistedAddressDao dao = DaoFactory.GetDao<IPersistedAddressDao>();

				PersistedAddressFilter filter = dao.GetFilter();

				filter.Fill(context.Request);

				SearchResult<PersistedAddress> persistedAddresses = dao.Find(filter);

				context.Request.Items[this.PersistedAddressItemNameOut] = persistedAddresses;

				retEvent = EventUtilities.GetSearchResultEventName(persistedAddresses.Items.Length);
			} catch (Exception ex) {
				this.logger.Error("Error occured in Execute.", ex);
			}

			return retEvent;
		}

		#endregion

		#region Nested type: Events

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