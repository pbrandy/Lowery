using Lowery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery30Demo.Models
{
	public class LookupAgency
	{
		[PrimaryKey]
		public long ObjectID { get; set; }

		[FieldName("agencygrouped")]
		public string AgencyGrouped { get; set; } = string.Empty;

		[FieldName("agencyabbr")]
		public string AgencyAbbr { get; set; } = string.Empty;

		public override string ToString()
		{
			return AgencyGrouped;
		}
	}
}
