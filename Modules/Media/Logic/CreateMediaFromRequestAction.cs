using System;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Logic.Support;
using Triton.Media.Support.Request;

namespace Triton.Media.Logic
{
	public class CreateMediaFromRequestAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();


		public CreateMediaFromRequestAction()
		{
			this.MediaItemNameOut = ItemNames.Media.DEFAULT_SEARCH_RESULT_MEDIA;
		}


		public string MediaItemNameOut { get; set; }

		#region IAction Members

		public string Execute(TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				Model.Media toReturn = Deserialize.CreateMedia(context.Request);

				context.Request.Items[this.MediaItemNameOut] = toReturn;

				retEvent = Events.Ok;
			} catch (Exception ex) {
				this.logger.Error("Error occurred in Execute.", ex);
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