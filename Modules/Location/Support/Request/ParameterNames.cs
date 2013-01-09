

namespace Triton.Location.Support.Request {


public class ParameterNames
{

	#region Nested type: Address

	public class Address
	{
		#region Nested type: Field

		public class Field
		{
			public const string ID = "address_id";

			public const string CITY = "city";

			public const string COUNTRY = "country";

		    public const string COUNTRY_ID = "country_id";

			public const string COUNTY = "county";

			public const string LINE1 = "address1";

			public const string LINE2 = "address2";

			public const string LINE3 = "address3";

			public const string POSTALCODE = "postalcode";

			public const string STATE = "state";

		    public const string STATE_ID = "state_id";
		}

		#endregion
	}

	#endregion

	#region Nested type: PersistedAddress

	/// <summary>
	/// Definitions for Fields and Filters of Persisted Address
	/// </summary>
	public class PersistedAddress 
	{
		#region Nested type: Field

		/// <summary>
		/// Fields for Persisted Address
		/// </summary>
		public class Field : Address.Field
		{
			public const string ID = "persisted_address_id";

			public const string CITY = "persisted_address_city_name";

			public const string COUNTRY = "persisted_address_country_name";

		    public const string COUNTRY_ID = "persisted_address_country_id";

			public const string COUNTY = "persisted_address_county";

			public const string LINE1 = "persisted_address_line1";

			public const string LINE2 = "persisted_address_line2";

			public const string LINE3 = "persisted_address_line3";

			public const string POSTALCODE = "persisted_address_postal_code_name";

			public const string STATE = "persisted_address_state_name";

		    public const string STATE_ID = "persisted_address_state_id";


		}

		#endregion

		#region Nested type: Filter
		/// <summary>
		/// Filters for Persisted Address
		/// </summary>
		public class Filter
		{
			public const string ID = "filter_address_id";

			public const string CITY = "filter_address_city_name";

			public const string COUNTRY = "filter_address_country_name";

			public const string COUNTY = "filter_address_county";

			public const string LINE1 = "filter_ddress_line1";

			public const string LINE2 = "filter_address_line2";

			public const string LINE3 = "filter_address_line3";

			public const string POSTALCODE = "filter_address_postal_code_name";

			public const string STATE = "filter_address_state_name";


		}

		#endregion
	}

	#endregion

}
}