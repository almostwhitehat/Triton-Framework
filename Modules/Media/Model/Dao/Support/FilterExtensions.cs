using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Triton.Model.Dao;
using Triton.Model.Dao.Support;
using Triton.Controller.Request;
using Triton.Media.Model.Dao;
using Triton.Utilities;

namespace Triton.Media.Model.Dao.Support
{
	/// <summary>
	/// 
	/// </summary>
	public static class FilterExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="request"></param>
		public static void Fill(this MediaFilter filter,
						MvcRequest request)
		{
			if (!string.IsNullOrEmpty(request["filter_media_id"])) {
				filter.Ids = request["filter_media_id"].ToIntArray();
			}



			if (!string.IsNullOrEmpty(request["filter_media_name"]) ){
				filter.Name = request["filter_media_name"];
			}

			((BaseFilter)filter).Fill(request);
		}

	}
}
