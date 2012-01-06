using System;

namespace Commanding.EventAggregator
{
    /// <summary>
    /// If you want to subscribe to a specific type of message you have
    /// to implement this interface on your subscriber class
    /// </summary>
    /// <typeparam name="TMessage">The <see cref="Type"/> of the message 
    /// you want to subscribe to</typeparam>
    public interface ISubscribeTo<in TMessage>
    {
        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Handle(TMessage message);
    }
}
