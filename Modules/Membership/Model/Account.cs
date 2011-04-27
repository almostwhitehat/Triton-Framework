using System;
using System.Linq;
using System.Collections.Generic;
using Triton.Location.Model;
using Triton.Membership.Support;

namespace Triton.Membership.Model {

	#region History

	// History:
	//   4/8/2011	SD	Added GetAttributeValue methods to simplify retrival of attribute values from the Account.

	#endregion

	public class Account
	{
		private string password;

		public virtual IDictionary<string, PersistedAddress> Addresses { get; set; }

		public virtual IDictionary<AttributeType, string> Attributes { get; set; }

		public virtual DateTime? CreateDate { get; set; }

		public virtual Guid? Id { get; set; }

		public virtual DateTime? ModifiedDate { get; set; }

		public virtual string Password
		{
			get { return this.password; }

			set { this.password = new EncryptionManager().EncryptField(value); }
		}

		public virtual Person Person { get; set; }

		public virtual IList<Role> Roles { get; set; }

		public virtual AccountStatus Status { get; set; }

		public virtual IList<Username> Usernames { get; set; }

		public virtual long? Version { get; set; }


		public virtual bool IsMemberOf(
			string roleName)
		{
			Role role = Roles.FirstOrDefault(x => x.Code == roleName);

			return role != null;
		}


		/// <summary>
		/// Returns the value of the account attribute with the
		/// given attribute code associated with the account.
		/// </summary>
		/// <param name="attributeCode">The code of the attribute to get the value for.</param>
		/// <returns>The value of the specified attribute, or <c>null</c> if the attribute is not found.</returns>
		public virtual string GetAttributeValue(
			string attributeCode)
		{
			string val = null;

			if (Attributes.Any(attr => attr.Key.Code == attributeCode)) {
				val = Attributes[Attributes.First(attr => attr.Key.Code == attributeCode).Key];
			}

			return val;
		}


		/// <summary>
		/// Returns the value of the account attribute with the
		/// given attribute ID associated with the account.
		/// </summary>
		/// <param name="attributeId">The ID of the attribute to get the value for.</param>
		/// <returns>The value of the specified attribute, or <c>null</c> if the attribute is not found.</returns>
		public virtual string GetAttributeValue(
			int attributeId)
		{
			string val = null;

			if (Attributes.Any(attr => attr.Key.Id.Value == attributeId)) {
				val = Attributes[Attributes.First(attr => attr.Key.Id.Value == attributeId).Key];
			}

			return val;
		}
	}
}