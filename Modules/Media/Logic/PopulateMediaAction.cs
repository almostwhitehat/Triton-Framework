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
	// 04/04/2012 msc	
	#endregion

	public class PopulateMediaAction : IAction
	{
		private const string EVENT_ERROR = "error";
		private const string EVENT_OK = "ok";

		public string MediaRequestName { get; set; }

		public PopulateMediaAction()
		{
			this.MediaRequestName = "media";
		}

		#region BizAction Members

		public string Execute(
			TransitionContext context)
		{
			string retEvent = EVENT_ERROR;

			try {
				MvcRequest request = context.Request;
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

				context.Request.Items[this.MediaRequestName] = media;


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

			////string originalFileName = this.GetFileName(file);
			//string fileName = originalFileName;

			//MediaType type;
			//IMediaTypeDao dao = DaoFactory.GetDao<IMediaTypeDao>();

			//IList<MediaType> types = dao.Get(new MediaType {
			//    FileTypes = new List<string> { this.GetFileType(fileName) }
			//});

			//if (types.Count > 0) {
			//    type = types[0];
			//}
			//else {
			//    type = dao.Get("misc_docs");
			//}

			//retMedia.Type = type;

			//string webPath = this.GetFilePathSuffix(string.Empty);

			//string filePath = Path.Combine(WebInfo.BasePath, webPath);

			//Directory.CreateDirectory(filePath);

			//retMedia.File = new FileRecord {
			//    Name = fileName,
			//    Path = webPath
			//};

			//retMedia.Name = originalFileName;

			//retMedia.CreatedDate = DateTime.Now;
			//retMedia.UpdatedDate = DateTime.Now;

			return retMedia;
		}
	}
}