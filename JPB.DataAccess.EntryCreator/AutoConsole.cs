/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using JPB.DataAccess.EntityCreator.Core.Models;
using JPB.DataAccess.EntityCreator.MsSql;

namespace JPB.DataAccess.EntityCreator
{
	public class AutoConsole
	{
		public AutoConsole(string path)
		{
			LoadStorage(path);
		}

		int index = 0;
		public Options Options { get; set; }
		private List<string> _op = new List<string>();

		public string GetNextOption()
		{
			if (Options == null)
				return SetNextOption();
			if (Options.Actions.Length > index)
				return Options.Actions[index++];
			return Console.ReadLine();
		}

		public string SetNextOption()
		{
			var action = Console.ReadLine();
			_op.Add(action);
			return action;
		}

		public void SetNextOption(string op)
		{
			_op.Add(op);
		}

		public void LoadStorage(string path)
		{
			if (path == null)
			{
				return;
			}
			using (var fs = new FileStream(path, FileMode.Open))
			{
				var serilizer = new XmlSerializer(typeof(Options));
				Options = serilizer.Deserialize(fs) as Options;
			}
		}

		public void SaveStorage(string path)
		{
			using (var fs = new FileStream("out.xml", FileMode.OpenOrCreate))
			{
				var serilizer = new XmlSerializer(typeof(Options));

				serilizer.Serialize(fs, new Options() { Actions = _op.ToArray() });
			}
		}
	}
}
