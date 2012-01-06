using System;

namespace Commanding.EventAggregator
{
    /// <summary>
    /// Provides the means for propagating messages in a loosely coupled manner. I.e.
    /// components do not have to know about each other, just about the messages they want to
    /// be able to exchange
    /// </summary>
    public interface IEventAggregator
    {
        /// <summary>
        /// Publishes a message to all its subscribers
        /// </summary>
        /// <typeparam name="TMessage">The <see cref="Type"/> of the message</typeparam>
        /// <param name="message">The message</param>
        void Publish<TMessage>(TMessage message);

        /// <summary>
        /// Subscribe to messages
        /// </summary>
        /// <param name="subscriber">The subscriber to register</param>
        void Subscribe(object subscriber);

        /// <summary>
        /// Cancel subscriptions
        /// </summary>
        /// <param name="subscriber">The subscriber to remove</param>
        void Unsubscribe(object subscriber);
    }
}
