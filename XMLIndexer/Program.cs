using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.IO;
using System.Xml.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Threading;
using System.Diagnostics;

namespace XMLIndexer
{

	class Program
	{
		[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
		static void Main(string[] args)
		{
			string documentpath = @"d:\requests\";
			if (!Directory.Exists(documentpath))
			{
				Directory.CreateDirectory(documentpath);
			}
			string indicespath = @"d:\requests\indices";
			if (!Directory.Exists(indicespath))
			{
				Directory.CreateDirectory(indicespath);
			}
			//CreateFiles();

			DocumentIndexer indexer = new DocumentIndexer(documentpath);


			Index<string> isMultiLead = null;
			Index<string> clientName = null;
			Index<int> projectId = null;
			Index<string> isDotNet = null;
			if (true)
			{
				Console.WriteLine("Rebuilding indices...");
				
				isMultiLead = new DatafieldIndex("IsMultiLead", indicespath, documentpath, "isMultiLead"); 
				clientName = new DatafieldIndex("ClientName", indicespath, documentpath, "clientname");
				projectId = new ProjectIndex("ProjectId", indicespath, documentpath);
				isDotNet = new DatafieldIndex("IsDotNet", indicespath, documentpath, "isDotNet");
				indexer.Register(isMultiLead);
				indexer.Register(clientName);
				indexer.Register(isDotNet);
				indexer.Register(projectId);

				Stopwatch sw = new Stopwatch();
				sw.Restart();
				indexer.CreateForAll();
				sw.Stop();
				WriteInColor(string.Format("Project Index rebuilt in: {0}", sw.ElapsedMilliseconds), ConsoleColor.Yellow);
				
			}
			else
			{
				Console.WriteLine("Loading indices...");
				Stopwatch sw = new Stopwatch();
				sw.Restart();
				isMultiLead = DatafieldIndex.Load(indicespath, "IsMultiLead").SetPropertyLocator((doc) =>
							{
								var elm = doc.Descendants("datafield").Where(i => i.Attribute("name").Value == "isMultiLead").FirstOrDefault();
								string value = String.Empty;
								if (elm != null)
								{
									value = elm.Value;
								}
								return value;
							});

				clientName = Index<string>.Load(indicespath, "ClientName").SetPropertyLocator((doc) =>
							{
								var elm = doc.Descendants("datafield").Where(i => i.Attribute("name").Value == "clientname").FirstOrDefault();
								string value = String.Empty;
								if (elm != null)
								{
									value = elm.Value;
								}
								return value;
							});

				projectId = Index<int>.Load(indicespath, "ProjectId")
				.SetPropertyLocator((doc) =>
				{
				  var value = int.Parse(doc.Root.Attribute("proj_id").Value);
				  return value;
				});

				isDotNet = Index<string>.Load(indicespath, "IsDotNet")
					.SetPropertyLocator((doc) =>
					{
						var elm = doc.Descendants("datafield").Where(i => i.Attribute("name").Value == "isDotNet").FirstOrDefault();
						string value = String.Empty;
						if (elm != null)
						{
							value = elm.Value;
						}
						return value;
					}); 
				sw.Stop();
				WriteInColor(string.Format("Indices loaded: {0}", sw.ElapsedMilliseconds), ConsoleColor.Yellow);
				indexer.Register(isMultiLead);
				indexer.Register(clientName);
				indexer.Register(projectId);
				indexer.Register(isDotNet);
			}
			
			



			//while (true)
			//{
			//  Console.WriteLine("Press spacebar to search for multileads and project 465...");
			//  //Console.WriteLine("Press C to search on clientname...");
			//  Console.WriteLine("Press Backspace to quit...");
			//  ConsoleKeyInfo bla = Console.ReadKey(true);
			//  if (bla.Key == ConsoleKey.Spacebar)
			//  {
			//    DoTheSearch(isMultiLead, projectId);
			//  }
			//  //else if (bla.Key == ConsoleKey.C)
			//  //{
			//  //  SearchClientName(clientName);
			//  //}
			//  else if (bla.Key == ConsoleKey.Backspace)
			//  {
			//    break;
			//  }
			//}

			Console.WriteLine("Search for non-DotNet of project id 220:");
			Stopwatch sw2 = new Stopwatch();
			sw2.Restart();
			List<int> trues = isDotNet.Search(s => s == "false" || string.IsNullOrEmpty(s)).ToList();
			List<int> projects = projectId.Search(s => s == 220).ToList();
			var combined = trues.Intersect(projects);

			
			sw2.Stop();
			WriteInColor(string.Format("Requests found: {0}", combined.Count()), ConsoleColor.Red);
			WriteInColor(string.Format("Ms elapsed: {0}", sw2.ElapsedMilliseconds), ConsoleColor.Yellow);

			Console.WriteLine("Generating document");
			Reporter reporter = new Reporter(documentpath, new List<string>() { "c_country", "n_country", "firstname", "clientname", "email","haschildren" , "tempstorage"}, @"d:\temp\requests.csv");
			reporter.Create(combined);


			Console.ReadKey();
		}
		private static void SearchClientName(Index<string> clientName)
		{
			Console.WriteLine("Search for clientname that starts with: (press enter when done...)");
			string search = Console.ReadLine();
			Stopwatch sw = new Stopwatch();
			sw.Restart();
			var items = clientName.Search(i => i.StartsWith(search));
			sw.Stop();
			WriteInColor(string.Format("Requests found: {0}", items.Count()), ConsoleColor.Red);
			WriteInColor(string.Format("Ms elapsed: {0}", sw.ElapsedMilliseconds), ConsoleColor.Yellow);
		}
		private static void DoTheSearch(Index<string> isMultiLead, Index<int> projectId)
		{
			Console.WriteLine("what project id? (press enter when done...)");
			int search = 0;
			
			if (int.TryParse(Console.ReadLine(), out search))
			{
				Console.WriteLine("Search for multileads of project id {0}:", search);
				Stopwatch sw = new Stopwatch();
				sw.Restart();
				
				List<int> trues = isMultiLead.Search(s => s == "true").ToList();
				List<int> projects = projectId.Search(s => s == search).ToList();

				Console.WriteLine("MultiLeads: {0}", trues.Count);
				Console.WriteLine("Requests of project {0}: {1}", search, projects.Count);
				var combined = trues.Intersect(projects);
				sw.Stop();

				WriteInColor(string.Format("Requests found: {0}", combined.Count()), ConsoleColor.Red);
				WriteInColor(string.Format("Ms elapsed: {0}", sw.ElapsedMilliseconds), ConsoleColor.Yellow);
			}
		}


		private static void WriteInColor(string text, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(text);
			Console.ResetColor();
		}
	}
}
