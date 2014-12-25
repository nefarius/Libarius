using System.Threading;

namespace Libarius.System
{
    public sealed class LimitInstance
    {
        private readonly string _instanceName;
        private Mutex _mutex;

        public LimitInstance(string instanceName)
        {
            _instanceName = instanceName;
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

        public void Dispose()
        {
            if (_mutex != null)
            {
                _mutex.Close();
            }
        }
    }
}