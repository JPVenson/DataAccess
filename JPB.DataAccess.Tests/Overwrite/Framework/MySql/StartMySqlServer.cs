using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JPB.DataAccess.Tests.Overwrite.Framework.MySql;
using NUnit.Framework;



namespace JPB.DataAccess.Tests.Overwrite.Framework.MySql
{
	public static class MySqlConnectorInstance
	{
		private static readonly Lazy<MySqlConnector> _instance;

		static MySqlConnectorInstance()
		{
			_instance = new Lazy<MySqlConnector>(() =>
			{
				var instance = new MySqlConnector();
				instance.Init(Path.Combine(TestContext.CurrentContext.TestDirectory, @"Dependencies\MySql\engine\mysqld.exe"));
				return instance;
			}, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		public static MySqlConnector Instance
		{
			get { return _instance.Value; }
		}
	}

	public class MySqlConnector
	{
		public MySqlConnector()
		{
			StopMySql = new ManualResetEventSlim();
			Listener = new ConcurrentDictionary<LoggerDelegate, LoggerDelegate>();
		}

		public void Init(string pathToEngineAssembly)
		{
			PathToEngineAssembly = pathToEngineAssembly;
			if (!File.Exists(PathToEngineAssembly))
			{
				throw new InvalidOperationException("The given MySql assembly path does not exist");
			}

			StopMySqlProcesses();

			var relPathToData = Path.Combine(Path.GetDirectoryName(PathToEngineAssembly), "..", "data");
			var pathToData = Path.GetFullPath(relPathToData);
			if (Directory.Exists(pathToData))
			{
				Directory.Delete(pathToData, true);
			}
		}

		private static void StopMySqlProcesses()
		{
			foreach (var process in Process.GetProcessesByName("mysqld"))
			{
				var trys = 0;
				do
				{
					trys++;
					try
					{
						process.Refresh();
						if (!process.HasExited)
						{
							process.Kill();
							process.WaitForExit();
						}

						break;
					}
					catch (Exception e)
					{
						Console.WriteLine($"Tried to kill MySql Task ... try {trys}");
					}
				} while (trys < 3);
			}
		}

		public string PathToEngineAssembly { get; private set; }

		public bool HasStarted { get; private set; }

		private enum StartCodes
		{
			UnkownError,
			CouldNotStart,
			Timeout,
			Ok
		}

		private string EmitDefaultValues()
		{
			var relPathToData = Path.Combine(Path.GetDirectoryName(PathToEngineAssembly), "..", "data");
			var pathToData = Path.GetFullPath(relPathToData);
			var sb = new StringBuilder();
			sb.Append($"--datadir=\"{pathToData}\" ");
			sb.Append("--port=\"55555\" ");
			sb.Append("--sql_mode=\"NO_ENGINE_SUBSTITUTION,STRICT_TRANS_TABLES\" ");
			sb.Append("--default_authentication_plugin=\"mysql_native_password\" ");
			sb.Append("--log-error=\"../share/error_log.err\" ");
			sb.Append("--user=\"root\" ");
			sb.Append("--log_syslog=0 ");
			sb.Append("--back_log=65535 ");
			return sb.ToString();
		}

		private async Task<StartCodes> ExecuteMySqlExecutable(Action<MySqlLogline> logRecived, string arguments)
		{
			var process = StartMySqlExecutable(arguments);
			return await Task.Run(async () =>
			{
				AttachLogParserToProcess(logRecived, process);
				AttachLogParserToProcessError(logRecived, process);
				if (!process.Start())
				{
					return StartCodes.CouldNotStart;
				}
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				process.WaitForExit();
				if (!process.WaitForExit((int)TimeSpan.FromMinutes(1).TotalMilliseconds))
				{
					return StartCodes.Timeout;
				}

				return StartCodes.Ok;
			});
		}

		private static void AttachLogParserToProcess(Action<MySqlLogline> logRecived, Process process)
		{
			var outputBuffer = new StringBuilder();
			process.OutputDataReceived += (sender, args) =>
			{
				outputBuffer.Append(args.Data);
				var linesParsed = MySqlLogline.ParseLogLine(outputBuffer.ToString());
				if (linesParsed.Loglines.Any())
				{
					outputBuffer = outputBuffer.Remove(0, linesParsed.LastCharConsumed);
					foreach (var linesParsedLogline in linesParsed.Loglines)
					{
						logRecived(linesParsedLogline);
					}
				}
			};
		}

		private static void AttachLogParserToProcessError(Action<MySqlLogline> logRecived, Process process)
		{
			var outputBuffer = new StringBuilder();
			process.ErrorDataReceived += (sender, args) =>
			{
				outputBuffer.Append(" " + args.Data);
				var linesParsed = MySqlLogline.ParseLogLine(outputBuffer.ToString());
				if (linesParsed.Loglines.Any())
				{
					var preOutput = outputBuffer.ToString().Substring(0,
						linesParsed.FirstCharConsumed);
					if (!string.IsNullOrWhiteSpace(preOutput))
					{
						logRecived(new MySqlLogline()
						{
							Orginal = preOutput,
							LogLevel = "ERROR"
						});
					}
					outputBuffer = outputBuffer.Remove(0, linesParsed.LastCharConsumed);
					foreach (var linesParsedLogline in linesParsed.Loglines)
					{
						logRecived(linesParsedLogline);
					}
				}
			};
		}

		private Process StartMySqlExecutable(string arguments)
		{
			var process = new Process();
			process.StartInfo = new ProcessStartInfo(PathToEngineAssembly);
			process.StartInfo.Arguments = arguments;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.EnableRaisingEvents = true;
			return process;
		}

		public enum CreateDatabaseResultCode
		{
			Unkown,
			SeeMessage,
			Error,
			Ok
		}

		public async Task<CreateDatabaseResultCode> CreateDatabaseFiles()
		{
			var logLines = new List<MySqlLogline>();
			var startMySqlExecutable = await ExecuteMySqlExecutable((logLine) => logLines.Add(logLine),
				EmitDefaultValues() + " --initialize-insecure --console");
			if (startMySqlExecutable != StartCodes.Ok)
			{
				return CreateDatabaseResultCode.SeeMessage;
			}

			if (logLines.Any(e => e.LogLevel.Equals("ERROR")))
			{
				return CreateDatabaseResultCode.Error;
			}

			var lastLogLine = logLines.Last();
			if (lastLogLine.Message.EndsWith("initializing of server has completed",
				StringComparison.InvariantCultureIgnoreCase))
			{
				return CreateDatabaseResultCode.Ok;
			}

			return CreateDatabaseResultCode.Error;
		}

		internal Task AwaitMySqlStop { get; set; }
		public ManualResetEventSlim StopMySql { get; private set; }
		Process startMySqlExecutable;

		private ConcurrentDictionary<LoggerDelegate, LoggerDelegate> Listener { get; }

		public class LoggerDelegate : IDisposable
		{
			private readonly Action _detatch;

			public LoggerDelegate(Action detatch)
			{
				_detatch = detatch;
				LogLines = new List<MySqlLogline>();
			}

			public event EventHandler<MySqlLogline> OnLogLine;

			public IList<MySqlLogline> LogLines { get; private set; }

			public void Dispose()
			{
				_detatch();
			}

			internal void OnOnLogLine(MySqlLogline e)
			{
				OnLogLine?.Invoke(this, e);
			}
		}

		private void DispatchLogLine(MySqlLogline logLine)
		{
			foreach (var loggerDelegate in Listener)
			{
				loggerDelegate.Value.LogLines.Add(logLine);
				loggerDelegate.Value.OnOnLogLine(logLine);
			}
		}

		public LoggerDelegate AttachLogger()
		{
			LoggerDelegate logger = null;
			logger = new LoggerDelegate(() => Listener.TryRemove(logger, out logger));
			return Listener.GetOrAdd(logger, logger);
		}

		public void CreateAndRunIfNot()
		{
			lock (this)
			{
				Path.GetFullPath()
				if (!HasStarted)
				{
					var createDatabaseResultCode = CreateDatabaseFiles().Result;
					if (createDatabaseResultCode != CreateDatabaseResultCode.Ok)
					{
						throw new InvalidOperationException("Could not create the Database files");
					}

					RunMySql();
				}
			}
		}

		public void RunMySql()
		{
			var logLines = new List<MySqlLogline>();
			startMySqlExecutable = StartMySqlExecutable(EmitDefaultValues() + " --console");
			HasStarted = true;
			startMySqlExecutable.Start();

			AttachLogParserToProcess(logLine =>
			{
				logLines.Add(logLine);
				if (logLine.LogLevel.Equals("ERROR", StringComparison.InvariantCultureIgnoreCase))
				{
					TestContext.Error.WriteLine(logLine.Orginal);
				}

				DispatchLogLine(logLine);
			}, startMySqlExecutable);

			AttachLogParserToProcessError(logLine =>
			{
				logLines.Add(logLine);
				if (logLine.LogLevel.Equals("ERROR", StringComparison.InvariantCultureIgnoreCase))
				{
					TestContext.Error.WriteLine(logLine.Orginal);
				}
				DispatchLogLine(logLine);
			}, startMySqlExecutable);

			AwaitMySqlStop = Task.Run(async () =>
			{
				StopMySql.Wait();
				await ExecuteMySqlExecutable((d) => { }, EmitDefaultValues() + " shutdown");

				if (!startMySqlExecutable.WaitForExit((int)TimeSpan.FromSeconds(3).TotalMilliseconds))
				{
					TestContext.Error.WriteLine("Could not stop MySql Process");
					StopMySqlProcesses();
				}
			});
		}
	}
}