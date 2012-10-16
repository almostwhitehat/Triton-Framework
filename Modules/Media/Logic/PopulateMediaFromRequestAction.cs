using System;
using Common.Logging;
using Triton;
using Triton.CodeContracts;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Logic.Support;
using Triton.Media.Model;
using Triton.Media.Support.Request;
using Triton.Model;

namespace Triton.Media.Logic
{
	public class PopulateMediaFromRequestAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();


		public PopulateMediaFromRequestAction()
		{
			this.MediaItemNameIn = ItemNames.Media.DEFAULT_SEARCH_RESULT_MEDIA;

			this.MediaItemNameOut = ItemNames.Media.DEFAULT_SEARCH_RESULT_MEDIA;
		}


		public string MediaItemNameIn { get; set; }

		public string MediaItemNameOut { get; set; }

		#region IAction Members

		public string Execute(TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
			
				ActionContract.Requires<NullReferenceException>(context.Request.Items[this.MediaItemNameIn] != null, "Could not retrieve the Media from the request to populate.");

				ActionContract.Requires<TypeMismatchException>(context.Request.Items[this.MediaItemNameIn] is Model.Media || context.Request.Items[this.MediaItemNameIn] is SearchResult<Model.Media>, "The Media item was not of type SearchResult<Media> or Media.");

				ActionContract.Requires<ApplicationException>(context.Request.Items[this.MediaItemNameIn] is Model.Media || (context.Request.Items[this.MediaItemNameIn] is SearchResult<Model.Media> && context.Request.GetItem<SearchResult<Model.Media>>(this.MediaItemNameIn).Items.Length > 0), "The Media items collection did not contain any items.");

				ActionContract.Requires<ApplicationException>(context.Request.Items[this.MediaItemNameIn] is Model.Media || (context.Request.Items[this.MediaItemNameIn] is SearchResult<Model.Media> && context.Request.GetItem<SearchResult<Model.Media>>(this.MediaItemNameIn).Items.Length == 1), "The Media items collection had more then one item, populate only works on one item at a time.");

				Model.Media media;

				if (context.Request.Items[this.MediaItemNameIn] is SearchResult<Model.Media>) {
					media = context.Request.GetItem<SearchResult<Model.Media>>(this.MediaItemNameIn).Items[0];
				} else {
					media = context.Request.GetItem<Model.Media>(this.MediaItemNameIn);
				}

				Model.Media toReturn = Deserialize.Populate(context.Request, media);

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