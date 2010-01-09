using System;
using System.Collections.Generic;
using System.Threading;

namespace Taste.Common
{
    /// <summary>
    /// Not really reentrant. Standard-issue lock to get project to compile. - cc
    /// </summary>
    public class ReentrantLock
    {
        private object syncObject;

        public ReentrantLock()
        {
            syncObject = new object();
        }

        public bool TryLock()
        {
            return Monitor.TryEnter(syncObject);
        }

        public bool TryLock(int millisecondTimeout)
        {
            return Monitor.TryEnter(syncObject, millisecondTimeout);
        }

        public void Lock()
        {
            Monitor.Enter(syncObject);
        }

        public void Unlock()
        {
            Monitor.Exit(syncObject);
        }
    }
}
