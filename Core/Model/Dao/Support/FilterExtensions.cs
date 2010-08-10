using Common.Logging;
using Triton.Controller.Request;

namespace Triton.Model.Dao.Support
{
	public static class FilterExtensions
	{
		public static void Fill(this BaseFilter filter,
		                        MvcRequest request)
		{
			if (!string.IsNullOrEmpty(request[BaseFilterParameterNames.PAGESIZE])) {
				int pagesize;
				if (int.TryParse(request[BaseFilterParameterNames.PAGESIZE], out pagesize)) {
					filter.PageSize = pagesize;
					filter.Page = 1;
				}
			}

			if (!string.IsNullOrEmpty(request[BaseFilterParameterNames.PAGE])) {
				if (filter.PageSize.HasValue) {
					int page;
					if (int.TryParse(request[BaseFilterParameterNames.PAGE], out page)) {
						filter.Page = page;
					}
				} else {
					LogManager.GetCurrentClassLogger().Warn(warn => warn("Call to set the page on filter, but since could not get pagesize the page is reset to null."));
				}
			}
		}
	}
}