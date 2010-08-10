namespace Triton.Location.Model
{
	public class Country
	{
		public virtual string Code { get; set; }

		public virtual string FullName { get; set; }

		public virtual string ShortName { get; set; }

		public virtual int? Id { get; set; }

		public virtual int? Version { get; set; }
	}
}