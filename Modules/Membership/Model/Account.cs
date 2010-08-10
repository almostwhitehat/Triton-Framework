using System;
using System.Collections.Generic;
using Triton.Location.Model;
using Triton.Membership.Support;
using System.Linq;

namespace Triton.Membership.Model
{
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

		public virtual bool IsMemberOf(string roleName)
		{
			Role role = Roles.FirstOrDefault(x => x.Code == roleName);

			return role != null;
		}
	}
}