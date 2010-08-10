using System;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Logic.Support;
using Triton.Membership.Model;
using Triton.Membership.Support.Request;
using Triton.Model;

namespace Triton.Membership.Logic
{
	public class PopulateAttributeTypeFromRequestAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();


		public PopulateAttributeTypeFromRequestAction()
		{
			this.AttributeTypeItemNameIn = ItemNames.AttributeType.DEFAULT_SEARCH_RESULT_ATTRIBUTE_TYPE;

			this.AttributeTypeItemNameOut = ItemNames.AttributeType.DEFAULT_SEARCH_RESULT_ATTRIBUTE_TYPE;
		}


		public string AttributeTypeItemNameIn { get; set; }

		public string AttributeTypeItemNameOut { get; set; }

		#region IAction Members

		public string Execute(TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				if (context.Request.Items[this.AttributeTypeItemNameIn] == null &&
				    !(context.Request.Items[this.AttributeTypeItemNameIn] is SearchResult<AttributeType>) &&
				    context.Request.GetItem<SearchResult<AttributeType>>(this.AttributeTypeItemNameIn).Items.Length != 1) {
					throw new MissingFieldException("Could not retrieve an attribute type or the more than one attribute type was returned.");
				}

				AttributeType account = context.Request.GetItem<SearchResult<AttributeType>>(this.AttributeTypeItemNameIn).Items[0];

				AttributeType toReturn = Deserialize.Populate(context.Request, account);

				SearchResult<AttributeType> result = new SearchResult<AttributeType>(new AttributeType[] {toReturn});

				context.Request.Items[this.AttributeTypeItemNameOut] = result;

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