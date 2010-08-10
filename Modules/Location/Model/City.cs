using System.Collections.Generic;

namespace Triton.Location.Model
{
	public class City
	{
		public virtual int? Id { get; set; }

		public virtual int? Version { get; set; }

		public virtual string Name { get; set; }

		public virtual IList<PostalCode> PostalCodes { get; set; }

		public virtual GeoLocation GeoLocation { get; set; }
	}
}