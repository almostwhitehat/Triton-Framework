using Triton.Model.Dao;

namespace Triton.Membership.Model.Dao
{
	public class RoleFilter : BaseFilter
	{
		public int[] Ids { get; set; }

		public string[] Codes { get; set; }
	}
}