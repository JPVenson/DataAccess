using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.TestModels.XmlDataRecordTest;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.XmlDataRecordTests
#if MsSql
.MsSQL
#endif

#if SqLite
.SqLite
#endif
{
	[TestFixture]
	public class XmlDataRecordTest
	{
		[Test]
		public void GetName()
		{
			var xmlSerilizer = new XmlSerializer(typeof (InstanceMock));
			string content = "";
			using (var ms = new MemoryStream())
			{
				xmlSerilizer.Serialize(ms, new InstanceMock());
				content = Encoding.Default.GetString(ms.ToArray());
			}

			var xmlRecord = new XmlDataRecord(content, typeof (InstanceMock).GetClassInfo());
			Assert.AreEqual(xmlRecord.GetName(0), "MockPropA");
			Assert.AreEqual(xmlRecord.GetName(1), "MockPropB");
		}

		[Test]
		public void GetValue()
		{
			var xmlSerilizer = new XmlSerializer(typeof (InstanceMock));
			string content = "";
			using (var ms = new MemoryStream())
			{
				xmlSerilizer.Serialize(ms, new InstanceMock());
				content = Encoding.Default.GetString(ms.ToArray());
			}

			var xmlRecord = new XmlDataRecord(content, typeof (InstanceMock).GetClassInfo());
			Assert.AreEqual(xmlRecord.GetValue(0), "NAN");
			Assert.AreEqual(xmlRecord.GetValue(1), 0);
		}

		[Test]
		public void InstanceFromString()
		{
			var xmlSerilizer = new XmlSerializer(typeof (InstanceMock));
			string content = "";
			using (var ms = new MemoryStream())
			{
				xmlSerilizer.Serialize(ms, new InstanceMock());
				content = Encoding.Default.GetString(ms.ToArray());
			}

			new XmlDataRecord(content, typeof (InstanceMock).GetClassInfo());
		}

		[Test]
		public void InstanceFromXDocument()
		{
			var xmlSerilizer = new XmlSerializer(typeof (InstanceMock));
			string content = "";
			using (var ms = new MemoryStream())
			{
				xmlSerilizer.Serialize(ms, new InstanceMock());
				content = Encoding.Default.GetString(ms.ToArray());
			}
			Assert.That(() => new XmlDataRecord(XDocument.Parse(content), typeof(InstanceMock), new DbConfig()), Throws.Nothing);
			;
		}
	}
}