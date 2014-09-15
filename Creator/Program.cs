using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SqlClient;

namespace Creator
{
	class Program
	{
		static void Main(string[] args)
		{
			CreateFiles(1);

		}

		private static void CreateFiles(int day)
		{
			
			using (var conn = new SqlConnection("Data Source=sqlbet001.tellus.local\\sqlbet001_04;Initial Catalog=Sellus;Integrated Security=True"))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand(string.Format("SELECT sl.lead_id, sl.project_specific_data FROM sellus_leads sl WITH (NOLOCK) WHERE proj_id = 220 and leaddate <= '2012-01-01' ORDER BY sl.leaddate", day), conn);
				cmd.CommandTimeout = 0;
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{

						int id = reader.GetInt32(reader.GetOrdinal("lead_id"));
						string data = reader.GetString(reader.GetOrdinal("project_specific_data"));
						File.WriteAllText(string.Format(@"d:\requests\{0}.xml", id), data);
					}
				}
			}
		}
	}
}
