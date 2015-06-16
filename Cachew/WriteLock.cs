using System;
using System.Threading;

namespace Cachew
{
    internal class WriteLock : IDisposable
    {
        private readonly ReaderWriterLockSlim readerWriterLockSlim;

        public WriteLock(ReaderWriterLockSlim readerWriterLockSlim)
        {
            if (readerWriterLockSlim == null) throw new ArgumentNullException("readerWriterLockSlim");
            this.readerWriterLockSlim = readerWriterLockSlim;

            readerWriterLockSlim.EnterWriteLock();
        }

        public void Dispose()
        {
            readerWriterLockSlim.ExitWriteLock();
        }
    }
}