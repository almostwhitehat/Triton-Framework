using Triton.Controller.Request;
using Triton.Membership.Support.Request;
using Triton.Model.Dao;
using Triton.Model.Dao.Support;
using Triton.Utilities;

namespace Triton.Membership.Model.Dao.Support
{
	public static class FilterExtensions
	{
		public static void Fill(this AccountFilter filter,
		                        MvcRequest request)
		{
			if (!string.IsNullOrEmpty(request[ParameterNames.Account.ID])) {
				filter.Ids = request[ParameterNames.Account.ID].ToGuidArray();
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.Account.USERNAME])) {
				filter.Usernames = request[ParameterNames.Account.USERNAME].ToStringArray();
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.Account.Filter.PERSONNAME])) {
				filter.Name = request[ParameterNames.Account.Filter.PERSONNAME];
			}

			((BaseFilter)filter).Fill(request);
		}


		public static void Fill(this RoleFilter filter,
		                        MvcRequest request)
		{
			if (!string.IsNullOrEmpty(request[ParameterNames.Role.ID])) {
				filter.Ids = request[ParameterNames.Role.ID].ToIntArray();
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.Role.CODE])) {
				filter.Codes = request[ParameterNames.Role.CODE].ToStringArray();
			}

			((BaseFilter)filter).Fill(request);
		}


		public static void Fill(this AttributeTypeFilter filter,
		                        MvcRequest request)
		{
			if (!string.IsNullOrEmpty(request[ParameterNames.AttributeType.ID])) {
				filter.Ids = request[ParameterNames.AttributeType.ID].ToIntArray();
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.AttributeType.CODE])) {
				filter.Codes = request[ParameterNames.AttributeType.CODE].ToStringArray();
			}

			((BaseFilter)filter).Fill(request);
		}
	}
}