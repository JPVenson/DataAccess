#region

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Tests.Base.TestModels.MetaAPI;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.MetaApiTests

{
	[TestFixture]
	[SuppressMessage("ReSharper", "LocalizableElement")]
	[Explicit]
	public class MetaApiSpeedTest
	{
		private TimeSpan Measure(Action target)
		{
			var sp = new Stopwatch();
			sp.Start();
			target();
			sp.Stop();
			return sp.Elapsed;
		}

		private void Measure(string text, Action target)
		{
			var sp = new Stopwatch();
			sp.Start();
			target();
			sp.Stop();
			Console.WriteLine("{0} -> {1}", text, sp.Elapsed);
		}

		private T Measure<T>(string text, Func<T> target)
		{
			var sp = new Stopwatch();
			sp.Start();
			var result = target();
			sp.Stop();
			Console.WriteLine("{0} -> {1}", text, sp.Elapsed);
			return result;
		}

		private TimeSpan OverallSpeedEvalPropertyGetAndSet(DbConfig dbInfo)
		{
			return Measure(() =>
			{
				var cache = dbInfo.GetOrCreateClassInfoCache(typeof(ClassSpeedMeasurement));
				var dbPropertyInfoCache = cache.Propertys["PropString"];

				ClassSpeedMeasurement instance = cache.DefaultFactory();
				dbPropertyInfoCache.Setter.Invoke(instance, "Value");
				var invoke = dbPropertyInfoCache.Getter.Invoke(instance);
			});
		}

		private TimeSpan OverallPlainSpeedEvalPropertyGetAndSet()
		{
			return Measure(() =>
			{
				var type = typeof(ClassSpeedMeasurement);
				var instance = Activator.CreateInstance(type);
				var propInfo = type.GetProperties().FirstOrDefault(s => s.Name == "PropString");
				propInfo.SetValue(instance, "Value");
				propInfo.GetValue(instance);
			});
		}

		[Test]
		public void OverallPlainSpeedEvalPropertyGetAndSetTest()
		{
			var timeSpan = Measure(() => OverallPlainSpeedEvalPropertyGetAndSet());
			Console.WriteLine("Time: {0}", timeSpan);
		}

		[Test]
		public void OverallPlainSpeedEvalPropertyGetAndSetTestOver100K()
		{
			var overallTime = new TimeSpan(0);
			var iterations = 100000;
			for (var i = 0; i < iterations; i++)
			{
				var timeEleapsed = OverallPlainSpeedEvalPropertyGetAndSet();
				overallTime = new TimeSpan(overallTime.Ticks + timeEleapsed.Ticks);
				Thread.Sleep(0);
			}

			Console.WriteLine("Over {0} with average of {1}", iterations, new TimeSpan(overallTime.Ticks / iterations));
		}

		[Test]
		public void OverallSpeedEvalPropertyGetAndSetTest()
		{
			DbConfig dbInfo = null;
			var timeSpan = Measure(() =>
			{
				dbInfo = new DbConfig(true);
				var cache = dbInfo.GetOrCreateClassInfoCache(typeof(ClassSpeedMeasurement));
				var dbPropertyInfoCache = cache.Propertys["PropString"];

				ClassSpeedMeasurement instance = cache.DefaultFactory();
				dbPropertyInfoCache.Setter.Invoke(instance, "Value");
				var invoke = dbPropertyInfoCache.Getter.Invoke(instance);
			});
			Console.WriteLine("Time: {0}", timeSpan);

			var followUp = Measure(() => OverallSpeedEvalPropertyGetAndSet(dbInfo));
			Console.WriteLine("Folowup Time: {0}", followUp);
		}

		[Test]
		public void OverallSpeedEvalPropertyGetAndSetTestOver100K()
		{
			var overallTime = new TimeSpan(0);
			var dbConfig = new DbConfig(true);
			var iterations = 100000;
			for (var i = 0; i < iterations; i++)
			{
				var timeEleapsed = OverallSpeedEvalPropertyGetAndSet(dbConfig);
				overallTime = new TimeSpan(overallTime.Ticks + timeEleapsed.Ticks);
				Thread.Sleep(0);
			}

			Console.WriteLine("Over {0} with average of {1}", iterations, new TimeSpan(overallTime.Ticks / iterations));
		}

		[Test]
		public void PlainSpeedEvalPropertyGetAndSet()
		{
			var timeSpan = Measure(() =>
			{
				var type = Measure("Get Type", () => typeof(ClassSpeedMeasurement));
				var instance = Measure("Object Creation", () => Activator.CreateInstance(type));
				var propInfo = Measure("Lookup Property",
					() => type.GetProperties().FirstOrDefault(s => s.Name == "PropString"));
				Measure("Set Value", () => propInfo.SetValue(instance, "Value"));
				Measure("Get Value", () => propInfo.GetValue(instance));
			});
			Console.WriteLine("Time: {0}", timeSpan);
		}

		[Test]
		public void SpeedEvalPropertyGetAndSet()
		{
			DbConfig dbInfo = null;
			var timeSpan = Measure(() =>
			{
				dbInfo = Measure("Generate DbConfig", () => new DbConfig(true));
				var cache = Measure("Generate Class Cache",
					() => dbInfo.GetOrCreateClassInfoCache(typeof(ClassSpeedMeasurement)));
				var dbPropertyInfoCache = Measure("Lookup Property", () => cache.Propertys["PropString"]);

				ClassSpeedMeasurement instance = Measure("Object Creation", () => cache.DefaultFactory());
				Measure("Set Value", () => dbPropertyInfoCache.Setter.Invoke(instance, "Value"));
				var invoke = Measure("Get Value", () => dbPropertyInfoCache.Getter.Invoke(instance));
			});
			Console.WriteLine("Time: {0}", timeSpan);

			var followUp = Measure(() =>
			{
				dbInfo = Measure("Generate DbConfig", () => new DbConfig(true));
				var cache = Measure("Generate Class Cache",
					() => dbInfo.GetOrCreateClassInfoCache(typeof(ClassSpeedMeasurement)));
				var dbPropertyInfoCache = Measure("Lookup Property", () => cache.Propertys["PropString"]);

				ClassSpeedMeasurement instance = Measure("Object Creation", () => cache.DefaultFactory());
				Measure("Set Value", () => dbPropertyInfoCache.Setter.Invoke(instance, "Value"));
				var invoke = Measure("Get Value", () => dbPropertyInfoCache.Getter.Invoke(instance));
			});
			Console.WriteLine("Reuse DbInfo Time: {0}", followUp);
		}
	}
}