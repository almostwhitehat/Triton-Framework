namespace Triton.Membership.Support.Request
{
	public class ParameterNames
	{
		#region Nested type: Account

		public class Account
		{
			#region Deprecated Parameter Names

			/// <summary>
			/// Deprecated. Please use the Field or Filter parameter names.
			/// </summary>
			public const string ID = "account_id";

			/// <summary>
			/// Deprecated. Please use the Field or Filter parameter names.
			/// </summary>
			public const string PASSWORD = "password";

			/// <summary>
			/// Deprecated. Please use the Field or Filter parameter names.
			/// </summary>
			public const string USERNAME = "username";

			/// <summary>
			/// Deprecated. Please use the Field or Filter parameter names.
			/// </summary>
			public const string ATTRIBUTE_VALUE = "attribute_value";

			/// <summary>
			/// Deprecated. Please use the Field or Filter parameter names.
			/// </summary>
			public const string AUTHENTICATED_ID = "authenticated_account_id";

			/// <summary>
			/// Deprecated. Please use the Field or Filter parameter names.
			/// </summary>
			public const string ATTRIBUTE_IS_CSV = "attribute_is_csv";

			#endregion

			#region Nested type: Field

			public class Field
			{
				public const string ID = "account_id";

				public const string PASSWORD = "account_password";

				public const string USERNAME = "account_username";

				public const string ATTRIBUTE_VALUE = "account_attribute_value";

				public const string ATTRIBUTE_IS_CSV = "account_attribute_is_csv";
			}
			
			#endregion			

			#region Nested type: Filter

			public class Filter
			{
				public const string ID = "filter_account_id";

				public const string PASSWORD = "filter_account_password";

				public const string USERNAME = "filter_account_username";

				public const string ATTRIBUTE_VALUE = "filter_account_attribute_value";

				public const string ATTRIBUTE_IS_CSV = "filter_account_attribute_is_csv";

				public const string PERSONNAME = "filter_account_person_name";
			}

			#endregion
		}

		#endregion

		#region Nested type: AttributeType

		public class AttributeType
		{
			public const string CODE = "attribute_type_code";

			public const string DESCRIPTION = "attribute_type_description";

			public const string ID = "attribute_type_id";

			public const string NAME = "attribute_type_name";

			public const string WEIGHT = "attribute_type_weight";
		}

		#endregion

		#region Nested type: NameSuffix

		public class NameSuffix
		{
			public const string ID = "suffix_id";
		}

		#endregion

		#region Nested type: Person

		public class Person
		{
			public const string EMAIL = "account_email";

			public const string FIRST_NAME = "first_name";

			public const string ID = "person_id";

			public const string LAST_NAME = "last_name";

			public const string MIDDLE_NAME = "middle_name";

			public const string PHONE = "account_phone";

			public const string PREFIX_CODE = "prefix_code";

			public const string SUFFIX_CODE = "suffix_code";
		}

		#endregion

		#region Nested type: Role

		public class Role
		{
			public const string CODE = "role_code";

			public const string DESCRIPTION = "role_description";

			public const string ID = "role_id";
		}

		#endregion

		#region Nested type: Account Status

		public class AccountStatus
		{
			public const string CODE = "account_status_code";
		}

		#endregion
		
	}
}