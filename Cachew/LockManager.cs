using System.Threading;

namespace Cachew
{
    internal class LockManager
    {
        private readonly ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();

        public ReadLock EnterRead()
        {
            return new ReadLock(readerWriterLockSlim);
        }

        public WriteLock EnterWrite()
        {
            return new WriteLock(readerWriterLockSlim);
        }
    }
}
