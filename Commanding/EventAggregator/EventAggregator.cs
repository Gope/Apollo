using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Commanding.EventAggregator
{
    /// <summary>
    /// An event aggregator that distributes messages from multiple sources to all 
    /// of their sinks
    /// </summary>
    public class EventAggregator : IEventAggregator
    {
        #region Fields

        private readonly List<WeakReference> subscribers;
        private readonly SynchronizationContext context;

        private readonly object syncRoot = new object();

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="EventAggregator"/> class.
        /// </summary>
        /// <param name="context">The <see cref="SynchronizationContext"/> used to marshal calls to the UI thread</param>
        public EventAggregator(SynchronizationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            this.context = context;
            this.subscribers = new List<WeakReference>();
        }

        #endregion Constructor

        #region Methods

        /// <inheritdoc/>
        public void Subscribe(object subscriber)
        {
            PurgeSubscribers();

            var canSubscribe = AllSubscribers()
                                    .Where(reference => reference.IsAlive &&
                                        reference.Target.Equals(subscriber))
                                    .Count() == 0;

            //only allow subscription if not subscribed yet
            if (canSubscribe)
            {
                WithinLock(() => subscribers.Add(new WeakReference(subscriber)));
            }
        }

        /// <inheritdoc/>
        public void Unsubscribe(object subscriber)
        {
            PurgeSubscribers();

            var query = from reference in AllSubscribers()
                        where reference.IsAlive &&
                            reference.Target.Equals(subscriber)
                        select reference;

            if (query.Count() > 0)
            {
                WithinLock(() => subscribers.Remove(query.First()));
            }
        }

        /// <inheritdoc/>
        public void Publish<TMessage>(TMessage message)
        {
            ISubscribeTo<TMessage> handler;

            foreach (var subscriber in AllSubscribers())
            {
                if (subscriber.IsAlive)
                {
                    handler = subscriber.Target as ISubscribeTo<TMessage>;
                    if (handler != null)
                    {
                        context.Send(state => handler.Handle(message), null);
                    }
                }
            }
        }

        private void PurgeSubscribers()
        {
            //query for dead connections
            var deadLinks = from subscriber in AllSubscribers()
                            where ((WeakReference)subscriber).IsAlive == false
                            select subscriber;

            WithinLock(() =>
                {
                    //remove them all
                    foreach (var link in deadLinks)
                    {
                        subscribers.Remove(link);
                    }
                });
        }

        private WeakReference[] AllSubscribers()
        {
            lock (syncRoot)
            {
                return subscribers.ToArray();
            }
        }

        private void WithinLock(Action action)
        {
            lock (syncRoot)
            {
                action();
            }
        }

        #endregion Methods
    }
}
