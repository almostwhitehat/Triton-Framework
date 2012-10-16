using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Triton.Controller.Request;
using Triton.Media.Model;
using Triton.Media.Model.Dao;
using Triton.Model.Dao;
using Triton.Web.Support;

namespace Triton.Media.Support.Request
{
	public class Deserialize
	{

		/// <summary>
		/// Creates a new <c>Media</c> from the request.
		/// </summary>
		/// <param name="request">Request to create the Media from.</param>
		/// <returns>A populated Media object.</returns>
		public static Model.Media CreateMedia(MvcRequest request)
		{


			return new Model.Media() {
				//TODO {INIT_PARAMS}
			};
		}


		/// <summary>
		/// Populates the <c>Media</c> with the values from the request.
		/// </summary>
		/// <param name="request">Request to populate the Media from.</param>
		/// <param name="media">Media to populate</param>
		/// <returns>Populated from the request Media object.</returns>
		public static Model.Media Populate(MvcRequest request,
									Model.Media media)
		{

			if (request[ParameterNames.Media.Field.NAME] != null) {
				media.Name = request[ParameterNames.Media.Field.NAME];
			}



			if (request[ParameterNames.Media.Field.CREATED_DATE] != null) {
				DateTime createdDate;
				if (DateTime.TryParse(request[ParameterNames.Media.Field.CREATED_DATE], out createdDate)) {
					media.CreatedDate = createdDate;
				}
			}



			if (request[ParameterNames.Media.Field.UPDATED_DATE] != null) {
				DateTime updatedDate;
				if (DateTime.TryParse(request[ParameterNames.Media.Field.UPDATED_DATE], out updatedDate)) {
					media.UpdatedDate = updatedDate;
				}
			}



			if (request[ParameterNames.Media.Field.COMMENTS] != null) {
				media.Comments = request[ParameterNames.Media.Field.COMMENTS];
			}



			if (request[ParameterNames.Media.Field.SORT_ORDER] != null) {
				float sortOrder;
				media.SortOrder = float.TryParse(request[ParameterNames.Media.Field.SORT_ORDER], out sortOrder) ? sortOrder : 0;
			}

			if (request[ParameterNames.Media.Field.FILE_NAME] != null && request[ParameterNames.Media.Field.FILE_PATH] != null) {
				string fileName = request[ParameterNames.Media.Field.FILE_NAME];
				string webPath = request[ParameterNames.Media.Field.FILE_PATH];

				//string filePath = Path.Combine(WebInfo.BasePath, webPath);

				media.File = new FileRecord {
					Name = fileName,
					Path = webPath
				};

				IMediaTypeDao dao = DaoFactory.GetDao<IMediaTypeDao>();

				IList<MediaType> types = dao.Get(new MediaType {
					FileTypes = new List<string> { GetFileType(fileName) }
				});

				media.Type = types.Count > 0 ? types[0] : dao.Get("misc_docs");
			}



			return media;
		}

		/// <summary>
		/// Strips any directory info and leaves only the file name.
		/// </summary>
		/// <param name="filePath">File path returned by the <code>WebPostedFile.Name</code></param>
		/// <returns>File name of the file.</returns>
		protected static string GetFileName(
			string filePath)
		{
			string fileName = filePath;

			if (filePath.Contains(@"\")) {
				string[] split = filePath.Split('\\');
				fileName = split[split.Length - 1];
			}

			return fileName;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="suffix"></param>
		/// <returns></returns>
		//protected string GetFilePathSuffix(
		//    string static suffix)
		//{
		//    foreach (Match match in Regex.Matches(suffix, @"[\[[\w]+]")) {
		//        string param = match.Value.Substring(1, match.Value.Length - 2);

		//        suffix = suffix.Replace(match.Value, this.request.Params[param]);
		//    }

		//    return suffix;
		//}

		protected static string GetFileType(
			string fileName)
		{
			string[] split = fileName.Split('.');
			return split[split.Length - 1];
		}

	}
}