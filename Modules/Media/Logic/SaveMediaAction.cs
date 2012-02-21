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

	public class SaveMediaAction : UploadMediaAction
	{
		private const string EVENT_ERROR = "error";
		private const string EVENT_OK = "ok";
		private const string USER_UPLOADED_FILES_SETTING = "userUploadedFiles";

		private MvcRequest request;

		private string requestItemName = "uploaded_media";
		//private string savePathSuffix;

		public string RequestItemName
		{
			get { return this.requestItemName; }
			set { this.requestItemName = value; }
		}

		public string MediaRequestName { get; set; }

		public SaveMediaAction()
		{
			this.MediaRequestName = "media_file_path";
		}

		#region BizAction Members

		public override string Execute(
			TransitionContext context)
		{
			string retEvent = EVENT_ERROR;

			try {
				this.request = context.Request;
				//idk how we got here and no files are in the request, and technically not an error

				IList<Model.Media> media = new List<Model.Media>();
				string[] files = context.Request[MediaRequestName].Split(',');
				foreach (string file in files) {
					media.Add(this.ProcessFile(file));
				}

				context.Request.Items["size"] = "Successfully uploaded: ";

				foreach (Model.Media m in media) {
					context.Request.Items["size"] += string.Format("{0}, ", m.Name);
				}

				context.Request.Items[this.RequestItemName] = media;


				context.Request.Items["result"] = "success";
				retEvent = EVENT_OK;
			}
			catch (Exception ex) {
				LogManager.GetLogger(typeof(UploadMediaAction)).Error(
							errorMessage => errorMessage("Error occured in UploadMediaAction.", ex));
				context.Request.Items["result"] = "error";
				context.Request.Items["error"] = "Internal error occured when uploading this file.";
			}

			return retEvent;
		}

		#endregion


		private Model.Media ProcessFile(
			string file)
		{
			Model.Media retMedia = new Model.Media();

			string originalFileName = this.GetFileName(file);
			string fileName = originalFileName;

			MediaType type;
			IMediaTypeDao dao = DaoFactory.GetDao<IMediaTypeDao>();

			IList<MediaType> types = dao.Get(new MediaType {
				FileTypes = new List<string> { this.GetFileType(fileName) }
			});

			if (types.Count > 0) {
				type = types[0];
			}
			else {
				type = dao.Get("misc_docs");
			}

			retMedia.Type = type;

			string webPath = this.GetFilePathSuffix(string.Empty);

			string filePath = Path.Combine(WebInfo.BasePath, webPath);

			Directory.CreateDirectory(filePath);

			retMedia.File = new FileRecord {
				Name = fileName,
				Path = webPath
			};

			retMedia.Name = originalFileName;

			retMedia.CreatedDate = DateTime.Now;
			retMedia.UpdatedDate = DateTime.Now;

			return retMedia;
		}
	}
}