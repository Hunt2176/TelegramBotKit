using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ActionLooper
{
    public class Looper
    {
        private static Looper? _looper = null;

        public static Looper GetMainLooper()
        {
            if (_looper != null) return _looper;

            _looper = new Looper(0);
            _looper.StartLoop();

            return _looper;
        }

        private SafeQueue<Action> _actionQueue = new SafeQueue<Action>();
        private ThreadLockContainer<Dictionary<string, Action>> _cycleActions = new ThreadLockContainer<Dictionary<string, Action>>(new Dictionary<string, Action>());
        
        private int _waitTime = 0;
        private int _taskDelay = 700;
        private bool _running = false;

        private void Initialize()
        {
            _running = true;
            
            Task.Run(() =>
            {
                while (_running)
                {
                    var queue = _actionQueue.DequeueAll();
                    _cycleActions.Do((x) =>
                    {
                        foreach (var value in x.Values)
                        {
                            queue.Enqueue(value);
                        }
                    });
                    while (queue.Count > 0)
                        try
                        {
                            queue.Dequeue()?.Invoke();
                            Thread.Sleep(_taskDelay);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        
                    Thread.Sleep(_waitTime);
                }
            });
        }

        /// <summary>
        /// Initiates the looper.
        /// Automatically called on initialization.
        /// </summary>
        public void StartLoop()
        {
            Initialize();
        }
        
        /// <summary>
        /// Stops the looper and releases threads.
        /// </summary>
        public void StopLoop()
        {
            _running = false;
        }
        
        /// <summary>
        /// Joins the Looper and does not release the thread until
        /// the looper has completed.
        /// </summary>
        public void Join()
        {
            while (_running)
            {
                Thread.Sleep(_waitTime);
            }
        }

        public bool IsRunning()
        {
            return _running;
        }

        /// <summary>
        /// Adds an action to the Loopers queue to be executed
        /// </summary>
        /// <param name="action">Action to be invoked</param>
        public void Post(Action action)
        {
            _actionQueue.Enqueue(action);
        }

        public void PostToCycleQueue(string key, Action action)
        {
            _cycleActions.Do((actions => { actions[key] = action; }));
        }

        /// <summary>
        /// Sets the wait time in-between idle wake-ups for the execution thread
        /// and the awaiting threads checking.
        /// </summary>
        /// <param name="waitTime">Time in milliseconds</param>
        public void SetIdleWait(int waitTime)
        {
            _waitTime = waitTime;
        }


        
        public Looper(int idleWaitTime)
        {
            SetIdleWait(idleWaitTime);
            StartLoop();
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
            T toReturn;
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

        public bool Contains(Predicate<T> predicate)
        {
            var contains = false;
            Monitor.Enter(_queue);
            try
            {
                foreach (var item in _queue)
                {
                    if (!predicate(item)) continue;
                    
                    contains = true;
                    break;
                }
            }
            finally
            {
                Monitor.Exit(_queue);
            }

            return contains;
        }

        public int Count()
        {
            return _queue.Count;
        }
    }

    class ThreadLockContainer<T>
    {
        private readonly T _object;
        
        public ThreadLockContainer(T baseObject)
        {
            _object = baseObject;
        }

        private void ObtainLock(Action<T> onObtain)
        {
            Monitor.Enter(_object);
            try
            {
                onObtain(_object);
            }
            finally
            {
                Monitor.Exit(_object);
            }
        }

        public void Do(Action<T> action)
        {
            ObtainLock(action);
        }
    }
}
