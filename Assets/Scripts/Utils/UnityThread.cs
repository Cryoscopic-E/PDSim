using System.Threading;
using UnityEngine;

namespace PDSim.Utils
{
    public abstract class UnityThread
    {
        private Thread _thread;
        protected bool IsRunning { get; private set; }

        protected abstract void Run();

        public void Start()
        {
            if (IsRunning)
                throw new UnityException("Thread started");
            Debug.Log("Thread started");
            IsRunning = true;
            _thread = new Thread(Run);
            _thread.Start();

        }

        public void Stop()
        {
            if (!IsRunning) return;
            IsRunning = false;
            Debug.LogWarning("Thread stopped");
            _thread.Join();
        }
    }
}