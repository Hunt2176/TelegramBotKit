using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ActionLooper
{
    public class Looper
    {
        private static Looper? _looper = null;

        public static Looper GetLooper()
        {
            if (_looper != null) return _looper;

            _looper = new Looper();
            _looper.StartLoop();

            return _looper;
        }

        private SafeQueue<Action> _actionQueue = new SafeQueue<Action>();
        private int _waitTime = 0;
        private bool _running = false;

        private void initialize()
        {
            _running = true;
            Task.Run(() =>
            {
                while (_running)
                {
                    if (_actionQueue.Count() > 0)
                    {
                        var queue = _actionQueue.DequeueAll();
                        while(queue.Count > 0)
                            queue.Dequeue()?.Invoke();
                    }
                        
                    Thread.Sleep(_waitTime);
                }
            });
        }

        public void StartLoop()
        {
            initialize();
        }

        public void StopLoop()
        {
            _running = false;
        }

        public void Post(Action action)
        {
            _actionQueue.Enqueue(action);
        }

        public void SetIdleWait(int waitTime)
        {
            _waitTime = waitTime;
        }
    }

    class SafeQueue<T>
    {
        private Queue<T> _queue = new Queue<T>();

        public void Enqueue(T item)
        {
            Monitor.Enter(_queue);

            try
            {
                _queue.Enqueue(item);
            }
            finally
            {
                Monitor.Exit(_queue);
            }
        }

        public Queue<T> DequeueAll()
        {
            Queue<T> allItems = new Queue<T>();
            Monitor.Enter(_queue);

            try
            {
                while(_queue.Count > 0)
                {
                    allItems.Enqueue(_queue.Dequeue());
                }
            }
            finally
            {
                Monitor.Exit(_queue);
            }

            return allItems;
        }

        public T Dequeue()
        {
            var toReturn = default(T);
            Monitor.Enter(_queue);

            try
            {
                toReturn = _queue.Dequeue();
            }
            finally
            {
                Monitor.Exit(_queue);
            }

            return toReturn;
        }

        public int Count()
        {
            return _queue.Count;
        }
    }
}
