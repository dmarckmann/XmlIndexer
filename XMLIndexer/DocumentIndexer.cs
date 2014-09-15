using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Xml.Linq;

namespace XMLIndexer
{


	public class DocumentIndexer
	{
		string _filepath;
		private FileSystemWatcher _watcher;
		private List<IIndex> indices = new List<IIndex>();
    public DocumentIndexer(string filepath)
		{
			_filepath = filepath;
			if (!Directory.Exists(filepath))
			{
				Directory.CreateDirectory(filepath);
			}

			_watcher = new FileSystemWatcher(filepath, "*.xml");
			_watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
					 | NotifyFilters.FileName | NotifyFilters.DirectoryName;

			_watcher.Changed += w_Changed;
			_watcher.Created += w_Created;
			_watcher.Deleted += w_Deleted;
			_watcher.EnableRaisingEvents = true;
		}

		void w_Deleted(object sender, FileSystemEventArgs e)
		{
			int id = int.Parse(e.Name.Replace(".xml", string.Empty));
			foreach (var index in indices)
			{
				index.OnDelete(id);
				index.Save();
			}
		}

		void w_Created(object sender, FileSystemEventArgs e)
		{
			int id = int.Parse(e.Name.Replace(".xml", string.Empty));
			XDocument doc = XDocument.Load(e.FullPath);
			foreach (var index in indices)
			{
				index.OnCreate(id, doc);
				index.Save();
			}
		}

		void w_Changed(object sender, FileSystemEventArgs e)
		{
			int id = int.Parse(e.Name.Replace(".xml", string.Empty));
			XDocument doc = XDocument.Load(e.FullPath);
			foreach (var index in indices)
			{
				index.OnChange(id, doc);
				index.Save();
			}
		}

		public void Register(IIndex index)
		{
			indices.Add(index);
		}

		public void CreateForAll()
		{
			int counter = 0;
			foreach (var file in Directory.GetFiles(_filepath, "*.xml"))
			{
				FileInfo fi = new FileInfo(file);

				int id = int.Parse(fi.Name.Replace(".xml", string.Empty));
				
				foreach (var index in indices)
				{
					index.OnCreate(id, XDocument.Load(file));
				}
				if (counter % 5000 == 0)
				{
					Console.WriteLine("ff saven bij {0}", counter);
					SaveIndices();
				}
				counter++;

			}
			SaveIndices();
		}
		
		private void SaveIndices()
		{
			foreach (var index in indices)
			{
				index.Save();
			}
		}	
	}



}
