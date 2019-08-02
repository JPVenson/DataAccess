using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace JPB.DataAccess.SqLite
{
	public static class SqLiteInteroptWrapper
	{
		public const string SqliteInteropDll = "SQLite.Interop.dll";
		public const string SqliteInterop64Dll = @"x64\" + SqliteInteropDll;
		public const string SqliteInterop32Dll = @"x86\" + SqliteInteropDll;

		public static string GetSqLitePath()
		{
			var pathTodll = SqliteInterop32Dll;

			if (Environment.Is64BitProcess)
			{
				pathTodll = SqliteInterop64Dll;
			}

			return pathTodll;
		}

		public static Stream GetSqLite()
		{
			var path = typeof(SqLiteInteroptWrapper).Namespace + "." + GetSqLitePath().Replace("\\", ".");
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
		}

		/// <summary>
		///		Ensures that the SqLite interopt lib is known and in a rechable location
		/// </summary>
		/// <returns>If the Interopt is existing or could be created in working directory</returns>
		public static bool EnsureSqLiteInteropt()
		{
			if (File.Exists(SqliteInteropDll))
			{
				return true;
			}

			var pathTodll = GetSqLitePath();

			//Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process) + ";" + pathTodll);
			//return true;
			var existingFile = new FileInfo(SqliteInteropDll);
			var newFile = new FileInfo("\\" + pathTodll);

			if (newFile.Exists)
			{
				if (existingFile.Exists)
				{
					if (existingFile.CreationTime <= newFile.CreationTime)
					{
						return true;
					}

					if (FileVersionInfo.GetVersionInfo(SqliteInteropDll).Equals(FileVersionInfo.GetVersionInfo(pathTodll)))
					{
						return true;
					}
				}

				try
				{
					File.Copy(pathTodll, SqliteInteropDll);
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}

			//try
			//{
			//	var path = typeof(SqLiteInteroptWrapper).Namespace + "." + pathTodll.Replace("\\", ".");
			//	using (var sqLite = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
			//	{
			//		if (sqLite == null)
			//		{
			//			return false;
			//		}

			//		var buffer = new byte[sqLite.Length];
			//		sqLite.Read(buffer, 0, buffer.Length);
			//		File.WriteAllBytes(SqliteInteropDll, buffer);
			//	}
			//}
			//catch (Exception)
			//{
			//	return false;
			//}

			return File.Exists(SqliteInteropDll);
		}
	}
}
