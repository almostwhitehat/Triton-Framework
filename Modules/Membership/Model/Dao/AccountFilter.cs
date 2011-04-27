using System;
using System.Collections.Generic;
using Triton.Model.Dao;

namespace Triton.Membership.Model.Dao {

	#region History

	//   4/6/2011	SD	Added ModifiedDate and CreatedDate properties.

	#endregion

	/// <summary>
	/// Defines the AccountFilter used to retrieve Accounts.
	/// </summary>
	public class AccountFilter : BaseFilter
	{
		/// <summary>
		/// Create a new AccountFilter.
		/// </summary>
		public AccountFilter()
		{
			this.Attributes = new Dictionary<string, string>();
		}

		/// <summary>
		/// Attributes to filter the results by.
		/// </summary>
		public IDictionary<string, string> Attributes { get; set; }

		/// <summary>
		/// Get and set the status id for the account filtering.
		/// </summary>
		public AccountStatus Status { get; set; }

		/// <summary>
		/// Get and set the account ids to retrieve.
		/// </summary>
		public Guid[] Ids { get; set; }

		/// <summary>
		/// Get the user names to retrieve.
		/// </summary>
		public string[] Usernames { get; set; }

		/// <summary>
		/// The Roles to filter the results by.
		/// </summary>
		public Role[] Roles { get; set; }

		///<summary>
		/// The Person.Name to Filter results by
		///</summary>
		public string Name { get; set; }

		///<summary>
		/// The date/time the account was last modified.
		/// This is a "since" date - to get accounts modified since this date.
		///</summary>
		public DateTime? ModifiedDate { get; set; }

		///<summary>
		/// The date/time the account was created.
		/// This is a "since" date - to get accounts created since this date.
		///</summary>
		public DateTime? CreatedDate { get; set; }
	}
}