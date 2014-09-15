using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace XMLIndexer
{
	public class Reporter
	{

		private readonly string _documentPath;
		private readonly List<string> _properties;
		private readonly string _filepath;
    public Reporter(string documentPath, List<string> properties, string filepath)
		{
			_filepath = filepath;
      _properties = properties;
      _documentPath = documentPath;			
		}

		public void Create(IEnumerable<int> ids)
		{
			TextWriter tw = new StreamWriter(_filepath);
			WriteHeaders(tw);
			
      
			foreach (var id in ids)
			{
				XDocument doc = XDocument.Load(Path.Combine(_documentPath, string.Format("{0}.xml", id)));
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("220,{0}", GetRequestDate(doc));
				foreach(var propertyname in _properties)
				{
					if (sb.Length > 0)
						sb.Append(",");
					sb.Append(GetPropertyValue(doc, propertyname));
				}
				tw.WriteLine(sb.ToString());
				tw.Flush();
			}
			
		}

		private string GetRequestDate(XDocument doc)
		{
			return string.Format("{0}-{1}-{2} {3}", doc.Root.Attribute("year").Value, doc.Root.Attribute("month").Value, doc.Root.Attribute("day").Value, doc.Root.Attribute("time").Value);
		}

		private void WriteHeaders(TextWriter tw)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("projectid, requestdate");
			foreach (var propertyname in _properties)
			{
				if (sb.Length > 0)
					sb.Append(",");
				sb.Append(propertyname);
			}
			tw.WriteLine(sb.ToString());
		}
		private string GetPropertyValue(XDocument doc, string propertyname)
		{
			var elm = doc.Descendants("datafield").Where(i => i.Attribute("name").Value == propertyname).FirstOrDefault();
			string value = String.Empty;
			if (elm != null)
			{
				value = elm.Value;
			}
			return value;
		}

	}
}
