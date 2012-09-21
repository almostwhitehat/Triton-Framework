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
using Triton.Logic.Support;
using Triton.Media.Model;
using Triton.Media.Model.Dao;
using Triton.Model;
using Triton.Model.Dao;
using Triton.Support.Request;
using Triton.Web.Support;


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

	public class DeleteMediaAction
	{
		private const string EVENT_ERROR = "error";
		private const string EVENT_OK = "ok";
		private const string USER_UPLOADED_FILES_SETTING = "userUploadedFiles";


		private string requestItemName = "uploaded_media";
		//private string savePathSuffix;

		public string RequestItemName
		{
			get { return this.requestItemName; }
			set { this.requestItemName = value; }
		}

		public string MediaRequestName { get; set; }

		public DeleteMediaAction()
		{
			this.MediaRequestName = "media_file_path";
		}

		#region BizAction Members

		public string Execute(
			TransitionContext context)
		{
			string retEvent = Events.Error;
			MvcRequest request = context.Request;

			try {
				IMediaDao dao = DaoFactory.GetDao<IMediaDao>();
				if (request.Items[MediaRequestName] is Triton.Media.Model.Media) {
					dao.Delete(request.GetRequestItem<Triton.Media.Model.Media>(MediaRequestName, true));
					this.ProcessFile(request.GetRequestItem<Triton.Media.Model.Media>(MediaRequestName, true));
				}
				else if (request.Items[MediaRequestName] is SearchResult<Triton.Media.Model.Media>) {
					SearchResult<Triton.Media.Model.Media> results = (SearchResult<Triton.Media.Model.Media>)request.Items[MediaRequestName];
					foreach (Triton.Media.Model.Media media in results.Items) {
						dao.Delete(media);
						this.ProcessFile(media);
					}
				}
				context.Request.Items["result"] = "success";
				retEvent = EVENT_OK;
			}
			catch (Exception ex) {
				LogManager.GetLogger(typeof(UploadMediaAction)).Error(
							errorMessage => errorMessage("Error occurred in DeleteMediaAction.", ex));
				context.Request.Items["result"] = "error";
				context.Request.Items["error"] = "Internal error occurred when uploading this file.";
			}

			return retEvent;
		}

		#endregion


		private void ProcessFile(
			Triton.Media.Model.Media media)
		{
			try {
				if (File.Exists(Path.Combine(media.File.Path, media.File.Name))) {
					File.Delete(Path.Combine(media.File.Path, media.File.Name));
				}
			}
			catch (Exception e) {
				ILog logger = LogManager.GetCurrentClassLogger();
				logger.Error(error => error("DeleteRelatedPurchasesAction - Error deleting related purchases from database."), e);
			}
		}

		#region Nested type: Events

		public class Events
		{
			public static string Ok
			{
				get
				{
					return EventNames.OK;
				}
			}

			public static string Error
			{
				get
				{
					return EventNames.ERROR;
				}
			}
		}

		#endregion

	}
}