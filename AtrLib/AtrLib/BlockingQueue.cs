using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace AtrLib
{
    public class BlockingQueue<T> : IBlockingQueue<T>, IDisposable
    {
        private readonly Queue<T> queue;
        private readonly object syncObj = new object();

        private readonly CancellationToken cancelationToken;
        private readonly ManualResetEvent queueEvent;

        private readonly WaitHandle[] handles;

        private bool disposed = false;

        #region .ctor & dispose        

        public BlockingQueue(CancellationToken cancelationToken)
            : this(new Queue<T>(), cancelationToken)
        {
        }

        public BlockingQueue(int capacity, CancellationToken cancelationToken)
            : this(new Queue<T>(capacity), cancelationToken)
        {
        }

        public BlockingQueue(IEnumerable<T> collection, CancellationToken cancelationToken)
            : this(new Queue<T>(collection), cancelationToken)
        {
        }

        protected BlockingQueue(Queue<T> queue, CancellationToken cancelationToken)
        {
            if (queue == null)
                throw new ArgumentNullException(nameof(queue));

            this.queue = queue;

            this.queueEvent = new ManualResetEvent(false);
            this.cancelationToken = cancelationToken;
            this.handles = new WaitHandle[] { cancelationToken.WaitHandle, queueEvent };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    lock (syncObj)
                    {
                        if (!disposed)
                        {
                            disposed = true;

                            queueEvent.Set();
                            queueEvent.Dispose();
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region IBlockingQueue<T>

        public T Pop()
        {
            T result = default(T);
            while (true)
            {
                lock (syncObj)
                {
                    if (disposed)
                        throw new ObjectDisposedException("BlockingQueue");

                    if (queue.Any())
                    {
                        result = queue.Dequeue();
                        if (!queue.Any())
                        {
                            if (!queueEvent.Reset())
                            {
                                throw new InvalidOperationException("Cannot reset queueEvent");
                            }
                            else
                            {
                                Trace.TraceInformation("[BlockingQueue] Reset queueEvent");
                            }
                        }
                        break;
                    }
                }

                int wr = WaitHandle.WaitAny(handles);
                if (wr == 0) //cancel
                {
                    Trace.TraceInformation("[BlockingQueue] Cancellation requested");
                    cancelationToken.ThrowIfCancellationRequested();
                }
                else
                {
                    Trace.TraceInformation($"[BlockingQueue] Wait complete, res = {wr}");
                }
            }
            return result;
        }

        public void Push(T item)
        {
            lock (syncObj)
            {
                if (disposed)
                    throw new ObjectDisposedException("BlockingQueue");

                bool isEmpty = !queue.Any();

                queue.Enqueue(item);

                if (isEmpty)
                {
                    if (!queueEvent.Set())
                    {
                        throw new InvalidOperationException("Cannot set queueEvent");
                    }
                    else
                    {
                        Trace.TraceInformation("[BlockingQueue] Set queueEvent");
                    }
                }
            }
        }

        #endregion
    }
}