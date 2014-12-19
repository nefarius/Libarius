using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Libarius.System
{
    public class LimitInstance
    {
        private string instanceName = string.Empty;
        private Mutex mutex;

        public LimitInstance(string instanceName)
        {
            this.instanceName = instanceName;
        }

        public void Dispose()
        {
            if (mutex != null)
            {
                mutex.Close();
            }
        }

        public bool IsOnlyInstance
        {
            get
            {
                try
                {
                    mutex = Mutex.OpenExisting(instanceName);
                    return false;
                }
                catch (WaitHandleCannotBeOpenedException)
                {
                    mutex = new Mutex(true, instanceName);
                    return true;
                }
            }
        }
    }
}
