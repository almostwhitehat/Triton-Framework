using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Controller.Request;
using Triton.Media.Model;
using Triton.Media.Model.Dao;
using Triton.Model.Dao;
using Triton.Web.Support;
using Triton.Media.Model.Dao.Support;
using Triton.Model;
using Triton.Logic.Support;

namespace Triton.Media.Logic
{

    #region History

    // History:
    // 08/13/2009	GV	Changed the inherited class to be Action after the rename from BizAction
    //					and change the DaoFactory.GetDao method name
    // 09/09/2009	GV	Changed the Action interface to inherit from IAction
    // 11/09/2009	KP	Added logic to resize an image on upload if user supplies the parameter in the request
    //					parameter syntax = "resizeimage=true".
    // 9/7/2010		SD	Changed file/directory path concatinations to use Path.Combine rather than "+".
    // 9/8/2011     MC  Added action option to include timestamp in filename when saved. defaulted to "yes"
    //                  any other value will not prepend the timestamp.
    #endregion

    public class GetMediaAction : IAction
    {
		private readonly ILog logger = LogManager.GetCurrentClassLogger();

		private const string EVENT_ERROR = "error";

        private MvcRequest request;

        private string requestItemName = "media";
        //private string savePathSuffix;

		public string MediaRequestItemNameOut
        {
            get { return this.requestItemName; }
            set { this.requestItemName = value; }
        }

        public GetMediaAction()
        {
        }

        #region BizAction Members

        public string Execute(
            TransitionContext context)
        {
            string retEvent = EVENT_ERROR;

			try {
				IMediaDao dao = DaoFactory.GetDao<IMediaDao>();

				MediaFilter filter = dao.GetFilter();

				filter.Fill(context.Request);

				SearchResult<Media.Model.Media> result = dao.Find(filter);

				context.Request.Items[this.MediaRequestItemNameOut] = result;

				retEvent = EventUtilities.GetSearchResultEventName(result.Items.Length);
			}
			catch (Exception ex) {
				this.logger.Error("Error occured in Execute.", ex);
			}

			return retEvent;
        }

        #endregion

    }
}