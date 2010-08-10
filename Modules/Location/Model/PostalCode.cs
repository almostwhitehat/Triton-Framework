namespace Triton.Location.Model
{
	public class PostalCode
	{
		public virtual int? Id { get; set; }

		public virtual int? Version { get; set; }

		public virtual string Number { get; set; }

		public virtual City City { get; set; }

		public virtual State State { get; set; }

		public virtual GeoLocation GeoLocation { get; set; }
	}
}