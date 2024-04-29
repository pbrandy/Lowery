namespace Lowery
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class GlobalIdentifier : Attribute
	{
		public string? FieldName { get; set; }
		public GlobalIdentifier()
		{

		}

		public GlobalIdentifier(string name)
		{
			FieldName = name;
		}
	}
}
