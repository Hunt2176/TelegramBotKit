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
        private SafeQueue<Tuple<int, Action>> _idQueue = new SafeQueue<Tuple<int, Action>>();
        
        private int _waitTime = 0;
        private int _taskDelay = 500;
        private bool _running = false;

        private void Initialize()
        {
            _running = true;
            
            Task.Run(() =>
            {
                while (_running)
                {
                    if (_actionQueue.Count() > 0)
                    {
                        var queue = _actionQueue.DequeueAll();
                        var idQueue = _idQueue.DequeueAll();
                        
                        foreach (var tuple in idQueue)
                        {
                            queue.Enqueue(tuple.Item2);
                        }
                        
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
        
        public void Post(int id, Action action)
        {
            if (!_idQueue.Contains((item) => item.Item1 == id))
            {
                _idQueue.Enqueue(new Tuple<int, Action>(id, action));
            }
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

    public class ActionQueue
    {
        private SafeQueue<Action> _actionQueue = new SafeQueue<Action>();
        private bool _looper = false;

        private void Initiate()
        {

            var instanceQueue = _actionQueue.DequeueAll();
            
            while (instanceQueue.Count > 0)
            {
                foreach (var action in instanceQueue)
                {
                    action.Invoke();
                    
                    if (_looper) _actionQueue.Enqueue(action);
                }
            }
        }

        public void Start()
        {
            Initiate();
        }

        public ActionQueue then(Action action)
        {
            _actionQueue.Enqueue(action);
            return this;
        }

        public ActionQueue UseAsLooper(bool looper)
        {
            _looper = looper;
            return this;
        }
        
        public ActionQueue() {}

        public ActionQueue(Action first)
        {
            _actionQueue.Enqueue(first);
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
                Console.WriteLine("Lock Acquired: Enqueue");
                _queue.Enqueue(item);
            }
            finally
            {
                Console.WriteLine("Lock Released: Enqueue");
                Monitor.Exit(_queue);
            }
        }

        public Queue<T> DequeueAll()
        {
            Queue<T> allItems = new Queue<T>();
            Monitor.Enter(_queue);

            try
            {
                Console.WriteLine("Lock Acquired: DequeueAll");
                while(_queue.Count > 0)
                {
                    allItems.Enqueue(_queue.Dequeue());
                }
            }
            finally
            {
                Console.WriteLine("Lock Released: DequeueAll");
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
                Console.WriteLine("Lock Acquired: Contains");
                foreach (var item in _queue)
                {
                    if (!predicate(item)) continue;
                    
                    contains = true;
                    break;
                }
            }
            finally
            {
                Console.WriteLine("Lock Released: Contains");
                Monitor.Exit(_queue);
            }

            return contains;
        }

        public int Count()
        {
            return _queue.Count;
        }
    }
}
