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
	public class CreateAttributeTypeAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();


		public CreateAttributeTypeAction()
		{
			this.AttributeTypeItemNameOut = ItemNames.AttributeType.DEFAULT_SEARCH_RESULT_ATTRIBUTE_TYPE;
		}


		public string AttributeTypeItemNameOut { get; set; }

		#region IAction Members

		public string Execute(TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				AttributeType toReturn = Deserialize.CreateAttributeType(context.Request);

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