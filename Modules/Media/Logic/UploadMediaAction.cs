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
    // 2/4/2014     MM  Changed timestamp string format to use MM for month
	#endregion

	public class UploadMediaAction : IAction
	{
		private const string EVENT_ERROR = "error";
		private const string EVENT_OK = "ok";
		private const string USER_UPLOADED_FILES_SETTING = "userUploadedFiles";

		private MvcRequest request;

		private string requestItemName = "uploaded_media";
		private string savePathSuffix;

		public string RequestItemName
		{
			get { return this.requestItemName; }
			set { this.requestItemName = value; }
		}

		public string SavePathSuffix
		{
			get { return this.savePathSuffix ?? ConfigurationManager.AppSettings[USER_UPLOADED_FILES_SETTING]; }
			set { this.savePathSuffix = value; }
		}

		public string IncludeTimeStamp { set; get; }

		public UploadMediaAction()
		{
			IncludeTimeStamp = "yes";
		}

		#region IAction Members

		public virtual string Execute(
			TransitionContext context)
		{
			string retEvent = EVENT_ERROR;

			try {
				this.request = context.Request;
				//idk how we got here and no files are in the request, and technically not an error
				if (context.Request.Files.Count == 0) {
					retEvent = EVENT_OK;
				}
				else {
					IList<Model.Media> media = new List<Model.Media>();

					foreach (string key in context.Request.Files.Keys) {
						media.Add(this.ProcessFile(context.Request.Files[key]));
					}

					context.Request.Items["size"] = "Successfully uploaded: ";

					foreach (Model.Media m in media) {
						context.Request.Items["size"] += string.Format("{0}, ", m.Name);
					}

					context.Request.Items[this.RequestItemName] = media;
				}

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
			MvcPostedFile file)
		{
			Model.Media retMedia = new Model.Media();

			string originalFileName = this.GetFileName(file.Name);
			string fileName = originalFileName;
			if (this.IncludeTimeStamp == "yes") {
				fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff-") + originalFileName;
			}

			MediaType type;
			IMediaTypeDao dao = DaoFactory.GetDao<IMediaTypeDao>();

			type = dao.Get(this.GetFileType(fileName));

			if (type == null) {
				type = dao.Get("misc_docs");
			}

			retMedia.Type = type;

			string webPath = this.GetFilePathSuffix(this.SavePathSuffix);

			string filePath = Path.Combine(WebInfo.BasePath, webPath);

			Directory.CreateDirectory(filePath);

			UploadMediaFilter filter = new UploadMediaFilter();
			filter.Fill(request);

			if ((filter.ResizeImage.HasValue)
					&& (filter.ResizeImage.Value == true)
					&& (filter.ResizeHeight.HasValue)
					&& (filter.ResizeWidth.HasValue)) {

				Bitmap image = this.GetResizedImage(
						file.InputStream,
						filter.ResizeWidth.Value,
						filter.ResizeHeight.Value);

				fileName = Path.GetFileNameWithoutExtension(fileName) + ".jpeg";
				image.Save(Path.Combine(filePath, fileName), ImageFormat.Jpeg);

			}
			else {
				file.SaveAs(Path.Combine(filePath, fileName));
			}

			retMedia.File = new FileRecord {
				Name = fileName,
				Path = webPath
			};
			retMedia.Name = originalFileName;
			retMedia.Comments = request["media_comments"];

			retMedia.CreatedDate = DateTime.Now;
			retMedia.UpdatedDate = DateTime.Now;

			return retMedia;
		}


		protected string GetFileType(
			string fileName)
		{
			string[] split = fileName.Split('.');
			return split[split.Length - 1];
		}


		/// <summary>
		/// Strips any directory info and leaves only the file name.
		/// </summary>
		/// <param name="filePath">File path returned by the <code>WebPostedFile.Name</code></param>
		/// <returns>File name of the file.</returns>
		protected string GetFileName(
			string filePath)
		{
			string fileName = filePath;

			if (filePath.Contains(@"\")) {
				string[] split = filePath.Split('\\');
				fileName = split[split.Length - 1];
			}

			return fileName;
		}


		protected string GetFilePathSuffix(
			string suffix)
		{
			foreach (Match match in Regex.Matches(suffix, @"[\[[\w]+]")) {
				string param = match.Value.Substring(1, match.Value.Length - 2);

				suffix = suffix.Replace(match.Value, this.request.Params[param]);
			}

			return suffix;
		}


		protected Bitmap GetResizedImage(
			Stream stream,
			int maxWidth,
			int maxHeight)
		{
			Bitmap src = Bitmap.FromStream(stream) as Bitmap;

			Bitmap newImage = this.ProportionallyResizeBitmap(src, maxWidth, maxHeight);

			return newImage;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="src"></param>
		/// <param name="maxWidth"></param>
		/// <param name="maxHeight"></param>
		/// <returns></returns>
		/// <remarks>http://absolutecobblers.blogspot.com/2008/02/aspnet-image-upload-and-resize-in-c-net.html</remarks>
		public Bitmap ProportionallyResizeBitmap(
			Bitmap src,
			double maxWidth,
			double maxHeight)
		{
			// original dimensions
			double w = src.Width;
			double h = src.Height;

			//// Longest and shortest dimension
			//double longestDimension = (w > h) ? w : h;
			//double shortestDimension = (w < h) ? w : h;

			//// propotionality
			//float factor = ((float)longestDimension)/shortestDimension;

			//// default width is greater than height
			//double newWidth = maxWidth;
			//double newHeight = maxWidth/factor;

			//// if height greater than width recalculate
			//if (w < h) {
			//    newWidth = maxHeight/factor;
			//    newHeight = maxHeight;
			//}

			double aspectRatio = w / h;
			double boxRatio = maxWidth / maxHeight;
			double scaleFactor = 0;

			if (boxRatio > aspectRatio) //Use height, since that is the most restrictive dimension of box.
				scaleFactor = maxHeight / h;
			else
				scaleFactor = maxWidth / w;

			double newWidth = w * scaleFactor;
			double newHeight = h * scaleFactor;
			// Create new Bitmap at new dimensions
			Bitmap result = new Bitmap((int)newWidth, (int)newHeight);
			using (Graphics g = Graphics.FromImage((Image)result)) {
				g.DrawImage(src, 0, 0, (int)newWidth, (int)newHeight);
			}

			return result;
		}
	}


	public class UploadMediaFilter
	{

		public bool? ResizeImage { get; set; }

		public int? ResizeWidth { get; set; }

		public int? ResizeHeight { get; set; }


		public void Fill(
			MvcRequest request)
		{

			bool resizeImage;
			if (bool.TryParse(request["resizeimage"], out resizeImage)) {
				ResizeImage = resizeImage;
			}

			int resizeWidth;
			if (int.TryParse(request["resizewidth"], out resizeWidth)) {
				ResizeWidth = resizeWidth;
			}

			int resizeHeight;
			if (int.TryParse(request["resizeheight"], out resizeHeight)) {
				ResizeHeight = resizeHeight;
			}
		}
	}
}