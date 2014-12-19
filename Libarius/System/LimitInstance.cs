using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Libarius.System
{
    public class LimitInstance
    {
        private readonly string _instanceName;
        private Mutex _mutex;

        public LimitInstance(string instanceName)
        {
            this._instanceName = instanceName;
        }

        public void Dispose()
        {
            if (_mutex != null)
            {
                _mutex.Close();
            }
        }

        public bool IsOnlyInstance
        {
            get
            {
                try
                {
                    _mutex = Mutex.OpenExisting(_instanceName);
                    return false;
                }
                catch (WaitHandleCannotBeOpenedException)
                {
                    _mutex = new Mutex(true, _instanceName);
                    return true;
                }
            }
        }
    }
}
