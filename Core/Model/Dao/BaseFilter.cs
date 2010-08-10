namespace Triton.Model.Dao
{
	public class BaseFilter
	{
		public int? Page { get; set; }

		public int? PageSize { get; set; }
	}


	public class BaseFilterParameterNames
	{
		public const string PAGE = "page";

		public const string PAGESIZE = "pagesize";
	}
}