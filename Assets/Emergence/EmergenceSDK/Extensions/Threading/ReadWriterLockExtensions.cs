using System;
using System.Threading;

namespace EmergenceSDK.Extensions.Threading
{
    public static class ReaderWriterLockExtensions
    {
        public static IDisposable ReadLock(this ReaderWriterLockSlim rwLock)
        {
            return new ReadLockDisposable(rwLock);
        }

        public static IDisposable WriteLock(this ReaderWriterLockSlim rwLock)
        {
            return new WriteLockDisposable(rwLock);
        }

        private class ReadLockDisposable : IDisposable
        {
            private readonly ReaderWriterLockSlim _rwLock;

            public ReadLockDisposable(ReaderWriterLockSlim rwLock)
            {
                _rwLock = rwLock;
                _rwLock.EnterReadLock();
            }

            public void Dispose()
            {
                _rwLock.ExitReadLock();
            }
        }

        private class WriteLockDisposable : IDisposable
        {
            private readonly ReaderWriterLockSlim _rwLock;

            public WriteLockDisposable(ReaderWriterLockSlim rwLock)
            {
                _rwLock = rwLock;
                _rwLock.EnterWriteLock();
            }

            public void Dispose()
            {
                _rwLock.ExitWriteLock();
            }
        }
    }
}