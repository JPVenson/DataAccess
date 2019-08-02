using System;

namespace JPB.DataAccess.SqLite
{
	public class DisposableAction : IDisposable
	{
		private readonly Action _obDisposing;

		public DisposableAction(Action obDisposing)
		{
			_obDisposing = obDisposing;
		}

		~DisposableAction()
		{
			ReleaseUnmanagedResources();
		}

		private void ReleaseUnmanagedResources()
		{
			_obDisposing();
		}

		public void Dispose()
		{
			ReleaseUnmanagedResources();
			GC.SuppressFinalize(this);
		}
	}
}