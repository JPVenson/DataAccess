using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.UnitTests.TestModels.XmlDataRecordTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace JPB.DataAccess.UnitTests
{
	[TestClass]
	public class XmlDataRecordTest
	{
		public XmlDataRecordTest()
		{

		}

		[TestMethod]
		public void InstanceFromString()
		{
			var xmlSerilizer = new XmlSerializer(typeof(InstanceMock));
			var content = "";
			using (var ms = new MemoryStream())
			{
				xmlSerilizer.Serialize(ms, new InstanceMock());
				content = Encoding.Default.GetString(ms.ToArray());
			}

			new XmlDataRecord(content, typeof(InstanceMock).GetClassInfo());
		}

		[TestMethod]
		public void InstanceFromXDocument()
		{
			var xmlSerilizer = new XmlSerializer(typeof(InstanceMock));
			var content = "";
			using (var ms = new MemoryStream())
			{
				xmlSerilizer.Serialize(ms, new InstanceMock());
				content = Encoding.Default.GetString(ms.ToArray());
			}

			new XmlDataRecord(XDocument.Parse(content), typeof(InstanceMock));
		}

		[TestMethod]
		public void GetName()
		{
			var xmlSerilizer = new XmlSerializer(typeof(InstanceMock));
			var content = "";
			using (var ms = new MemoryStream())
			{
				xmlSerilizer.Serialize(ms, new InstanceMock());
				content = Encoding.Default.GetString(ms.ToArray());
			}

			var xmlRecord = new XmlDataRecord(content, typeof(InstanceMock).GetClassInfo());
			Assert.AreEqual(xmlRecord.GetName(0), "MockPropA");
			Assert.AreEqual(xmlRecord.GetName(1), "MockPropB");
		}

		[TestMethod]
		public void GetValue()
		{
			var xmlSerilizer = new XmlSerializer(typeof(InstanceMock));
			var content = "";
			using (var ms = new MemoryStream())
			{
				xmlSerilizer.Serialize(ms, new InstanceMock());
				content = Encoding.Default.GetString(ms.ToArray());
			}

			var xmlRecord = new XmlDataRecord(content, typeof(InstanceMock).GetClassInfo());
			Assert.AreEqual(xmlRecord.GetValue(0), "NAN");
			Assert.AreEqual(xmlRecord.GetValue(1), 0);
		}
	}
}
