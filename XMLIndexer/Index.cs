using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace XMLIndexer
{
	public interface IIndex
	{
		void OnDelete(int id);
		void OnCreate(int id, XDocument doc);
		void OnChange(int id, XDocument doc);
		void Save();
	}
	[Serializable]
	public class Index<T> : IIndex
	{
		[NonSerialized]
		protected Func<XDocument, T> _propertyLocator;
  private readonly string _indicespath;
  private readonly string _documentpath;
  
		public  Index<T> SetPropertyLocator(Func<XDocument, T> propertyLocator)
		{
			_propertyLocator = propertyLocator;
			return this;
		}

		public void OnDelete(int id)
		{
			Data.Remove(id);
			
		}

		public void OnCreate(int id, XDocument doc)
		{
			CreateFor(id, doc);
			
		}

		public void OnChange(int id, XDocument doc)
		{
			Data.Remove(id);
			CreateFor(id, doc);
			
		}

		public Index(string name, string indicespath, string documentpath)
		{
			_indicespath = indicespath;
			_documentpath = documentpath;
			Name = name;
			Data = new Dictionary<int, T>();
		}
		public string Name { get; set; }
		public Dictionary<int, T> Data { get; set; }

		public void Save()
		{
			using (Stream stream = File.Create(Path.Combine(_indicespath, Name)))
			{
				new BinaryFormatter().Serialize(stream, this);
			}
		}

		public static Index<T> Load( string indicespath, string name)
		{

			using (Stream stream = File.OpenRead(Path.Combine(indicespath, name)))
			{
				Index<T> result = (Index<T>)new BinaryFormatter().Deserialize(stream);
				return result;
			}
		}

		private T GetValue(XDocument doc)
		{
			T value = _propertyLocator.Invoke(doc);
			return value;
		}
		private void CreateFor(int id, XDocument doc)
		{
			T value = GetValue(doc);
			Data.Add(id, value);
		}

		//private void CreateFor(string file)
		//{
		//  FileInfo info = new FileInfo(file);
		//  XDocument doc = XDocument.Load(info.FullName);
		//  T value = GetValue(doc);
		//  int id = int.Parse(info.Name.Replace(".xml", string.Empty));

		//  Data.Add(id, value);
		//}
		//public void CreateForAll()
		//{
		//  foreach (var file in Directory.GetFiles(_documentpath, "*.xml"))
		//  {
		//    CreateFor(file);
		//  }
		//  Save();
		//}

		public List<int> Search(Predicate<T> where)
		{
			var bla = Data.Where(s => where(s.Value)).Select(s => s.Key).ToList();
			return bla;
		}
	}

	[Serializable]
	public class DatafieldIndex : Index<string>
	{
		public DatafieldIndex(string name, string indicespath, string documentpath, string propertyname)
			: base(name, indicespath, documentpath)
		{
			_propertyLocator = (doc) =>
							{
								var elm = doc.Descendants("datafield").Where(i => i.Attribute("name").Value == propertyname).FirstOrDefault();
								string value = String.Empty;
								if (elm != null)
								{
									value = elm.Value;
								}
								return value;
							};
		}
	}
	[Serializable]
	public class ProjectIndex : Index<int>
	{
		
		public ProjectIndex(string name, string indicespath, string documentpath)
			: base(name, indicespath, documentpath)
		{
			_propertyLocator = (doc) =>
							{
								var value = int.Parse(doc.Root.Attribute("proj_id").Value);
								return value;
							};
		}

	}
}
