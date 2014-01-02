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


namespace Triton.Media.Logic {

#region History

// History:
// 08/13/2009	GV	Changed the inherited class to be Action after the rename from BizAction
//					and change the DaoFactory.GetDao method name
// 09/09/2009	GV	Changed the Action interface to inherit from IAction
// 11/09/2009	KP	Added logic to resize an image on upload if user supplies the parameter in the request
//					parameter syntax = "resizeimage=true".
//   9/7/2010	SD	Changed file/directory path concatinations to use Path.Combine rather than "+".
//   9/8/2011	MC  Added action option to include timestamp in filename when saved. defaulted to "yes"
//                  any other value will not prepend the timestamp.
//   1/2/1014	SD	Fixed to implement IAction. Clean up of unused/unneeded code.
#endregion

public class DeleteMediaAction : IAction
{


	/// <summary>
	/// Gets or sets the name of the item in Request.Items containing
	/// the Media to delete.
	/// </summary>
	public string MediaItemNameIn
	{
		get;
		set;
	}
	/// <summary>
	/// Derpicated. Use MediaItemNameIn instead.
	/// </summary>
	public string MediaRequestName
	{
		get {
			return MediaItemNameIn;
		}
		set {
			MediaItemNameIn = value;
		}
	}


	/// <summary>
	/// Gets or sets flag indicating whether or not the actions should
	/// also delete the file referenced by the media record.
	/// </summary>
	public bool DeleteFile
	{
		get;
		set;
	}

	
	#region BizAction Members

	public string Execute(
		TransitionContext context)
	{
		string retEvent = Events.Error;
		MvcRequest request = context.Request;

		try {
			IMediaDao dao = DaoFactory.GetDao<IMediaDao>();
			if (request.Items[MediaItemNameIn] is Model.Media) {
				dao.Delete(request.GetRequestItem<Model.Media>(MediaItemNameIn, true));
				if (DeleteFile) {
					DeleteMediaFile(request.GetRequestItem<Model.Media>(MediaItemNameIn, true));
				}
			} else if (request.Items[MediaItemNameIn] is SearchResult<Model.Media>) {
				SearchResult<Model.Media> results = (SearchResult<Model.Media>)request.Items[MediaItemNameIn];
				foreach (Model.Media media in results.Items) {
					dao.Delete(media);
					if (DeleteFile) {
						DeleteMediaFile(media);
					}
				}
			}

			context.Request.Items["result"] = "success";

			retEvent = Events.Ok;
		} catch (Exception e) {
			LogManager.GetLogger(typeof(UploadMediaAction)).Error(
					errorMessage => errorMessage("Error occurred in DeleteMediaAction.", e));
			context.Request.Items["result"] = "error";
			context.Request.Items["error"] = "Internal error occurred when deleting this file.";
		}

		return retEvent;
	}

	#endregion


	private void DeleteMediaFile(
		Model.Media media)
	{
		try {
			if (File.Exists(Path.Combine(media.File.Path, media.File.Name))) {
				File.Delete(Path.Combine(media.File.Path, media.File.Name));
			}
		} catch (Exception e) {
			ILog logger = LogManager.GetCurrentClassLogger();
			logger.Error(error => error("Error occurred deleting media."), e);
		}
	}


	#region Nested type: Events

	public class Events
	{
		public static string Ok
		{
			get {
				return EventNames.OK;
			}
		}

		public static string Error
		{
			get {
				return EventNames.ERROR;
			}
		}
	}

	#endregion

}
}