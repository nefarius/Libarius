using System;
using System.Threading;

namespace Libarius.System
{
    /// <summary>
    ///     Provides a check if a named instance already exists on the current system.
    /// </summary>
    public sealed class LimitInstance : IDisposable
    {
        private Mutex _mutex;
        private readonly string _instanceName;

        /// <summary>
        ///     Creates new named instance check.
        /// </summary>
        /// <param name="instanceName">The name of the instance to create or check.</param>
        public LimitInstance(string instanceName)
        {
            _instanceName = instanceName;
        }

        /// <summary>
        ///     Checks is the current named instance is the only one on the system.
        /// </summary>
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

        /// <summary>
        ///     Releases the underlying mutex.
        /// </summary>
        public void Dispose()
        {
            if (_mutex != null)
            {
                _mutex.Close();
            }
        }
    }
}