using System;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Logic.Support;
using Triton.Membership.Model;
using Triton.Membership.Model.Dao;
using Triton.Membership.Support.Request;
using Triton.Model;
using Triton.Model.Dao;

namespace Triton.Membership.Logic
{
	public class SaveAttributeTypeAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();


		public SaveAttributeTypeAction()
		{
			this.AttributeTypeItemNameIn = ItemNames.AttributeType.DEFAULT_SEARCH_RESULT_ATTRIBUTE_TYPE;
		}


		public string AttributeTypeItemNameIn { get; set; }

		#region IAction Members

		public string Execute(TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				if (context.Request.Items[this.AttributeTypeItemNameIn] == null &&
				    !(context.Request.Items[this.AttributeTypeItemNameIn] is SearchResult<AttributeType>) &&
				    context.Request.GetItem<SearchResult<AttributeType>>(this.AttributeTypeItemNameIn).Items.Length == 0) {
					throw new MissingFieldException("Could not retrieve an roles to save.");
				}

				SearchResult<AttributeType> attributeTypes = context.Request.GetItem<SearchResult<AttributeType>>(this.AttributeTypeItemNameIn);

				IAttributeTypeDao dao = DaoFactory.GetDao<IAttributeTypeDao>();

				dao.Save(attributeTypes.Items);

				retEvent = Events.Ok;
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