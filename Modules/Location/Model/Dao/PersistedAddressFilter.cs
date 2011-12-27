using System;
using Triton.Model.Dao;

namespace Triton.Location.Model.Dao
{
	public class PersistedAddressFilter : BaseFilter
	{
		public long[] Ids { get; set; }

		public string Line1 { get; set; }

		public string State { get; set; }

		public string Country { get; set; }

		public string City { get; set; }

		public string PostalCode { get; set; }
	}
}