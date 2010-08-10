using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GFW.Membership.model
{
	public class AccountAddress
	{
		public virtual int Id { get; set; }

		public virtual int Version { get; set; }

		public virtual AddressType AddressType { get; set; }

		public virtual PersistedAddress Address { get; set; }
	}
}
