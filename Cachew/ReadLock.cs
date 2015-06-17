using System;
using System.Threading;

namespace Cachew
{
    internal class ReadLock : IDisposable
    {
        private readonly ReaderWriterLockSlim readerWriterLockSlim;

        public ReadLock(ReaderWriterLockSlim readerWriterLockSlim)
        {
            if (readerWriterLockSlim == null) throw new ArgumentNullException("readerWriterLockSlim");
            this.readerWriterLockSlim = readerWriterLockSlim;

            readerWriterLockSlim.EnterReadLock();
        }

        public void Dispose()
        {
            readerWriterLockSlim.ExitReadLock();
        }
    }
}