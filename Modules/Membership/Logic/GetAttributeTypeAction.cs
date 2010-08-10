using System;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Logic.Support;
using Triton.Membership.Model;
using Triton.Membership.Model.Dao;
using Triton.Membership.Model.Dao.Support;
using Triton.Membership.Support.Request;
using Triton.Model;
using Triton.Model.Dao;

namespace Triton.Membership.Logic
{
	public class GetAttributeTypeAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();


		public GetAttributeTypeAction()
		{
			this.AttributeTypeItemNameOut = ItemNames.AttributeType.DEFAULT_SEARCH_RESULT_ATTRIBUTE_TYPE;
			this.All = false;
		}


		public string AttributeTypeItemNameOut { get; set; }

		public bool All { get; set; }

		#region IAction Members

		public string Execute(TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				IAttributeTypeDao dao = DaoFactory.GetDao<IAttributeTypeDao>();

				AttributeTypeFilter filter = dao.GetFilter();

				//if all is not set, filter results
				if (!this.All) {
					filter.Fill(context.Request);
				}

				SearchResult<AttributeType> attributeTypes = dao.Find(filter);

				context.Request.Items[this.AttributeTypeItemNameOut] = attributeTypes;

				retEvent = EventUtilities.GetSearchResultEventName(attributeTypes.Items.Length);
			} catch (Exception ex) {
				this.logger.Error("Error occured in Execute.", ex);
			}

			return retEvent;
		}

		#endregion

		#region Nested type: Events

		public class Events
		{
			public static string Ok
			{
				get { return EventNames.OK; }
			}

			public static string Error
			{
				get { return EventNames.ERROR; }
			}
		}

		#endregion
	}
}