using Triton.Controller.Request;
using Triton.Location.Support.Request;
using Triton.Model.Dao;
using Triton.Model.Dao.Support;
using Triton.Utilities;

namespace Triton.Location.Model.Dao.Support
{
	public static class FilterExtensions
	{
		public static void Fill(this PersistedAddressFilter filter,
		                        MvcRequest request)
		{
			if (!string.IsNullOrEmpty(request[ParameterNames.PersistedAddress.Filter.ID])) {
				filter.Ids = request[ParameterNames.PersistedAddress.Filter.ID].ToLongArray();
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.PersistedAddress.Filter.LINE1])) {
				filter.Line1 = request[ParameterNames.PersistedAddress.Filter.LINE1];
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.PersistedAddress.Filter.CITY])) {
				filter.City = request[ParameterNames.PersistedAddress.Filter.CITY];
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.PersistedAddress.Filter.STATE])) {
				filter.State = request[ParameterNames.PersistedAddress.Filter.STATE];
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.PersistedAddress.Filter.COUNTRY])) {
				filter.Country = request[ParameterNames.PersistedAddress.Filter.COUNTRY];
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.PersistedAddress.Filter.POSTALCODE])) {
				filter.PostalCode = request[ParameterNames.PersistedAddress.Filter.POSTALCODE];
			}

			((BaseFilter)filter).Fill(request);
		}


	}
}