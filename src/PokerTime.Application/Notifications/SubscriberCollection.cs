namespace PokerTime.Application.Notifications {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// This is essentially a class that does not take a hard reference on its subscribers while taking concurrency into account
    /// </summary>
    internal sealed class SubscriberCollection<TSubscriber> : IDisposable where TSubscriber : class, ISubscriber {
        private readonly ConcurrentDictionary<Guid, WeakReference<TSubscriber>> _subscribers = new ConcurrentDictionary<Guid, WeakReference<TSubscriber>>();
        private readonly ReaderWriterLockSlim _subscriberCollectionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public void Subscribe(TSubscriber subscriber) {
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
            _subscribers.TryAdd(subscriber.UniqueId, new WeakReference<TSubscriber>(subscriber));
        }

        public void Unsubscribe(TSubscriber subscriber) {
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
            _subscribers.TryRemove(subscriber.UniqueId, out _);
        }

        public IEnumerable<TSubscriber> GetItems() {
            // While we iterate through the queue we need to take note of any dead subscribers
            var deadSubscribers = new List<Guid>();

            // We need to take a read lock on the queue so at least stuff does not get removed while iterating
            _subscriberCollectionLock.EnterReadLock();
            try {
                foreach (var subscriberItem in _subscribers) {
                    if (!subscriberItem.Value.TryGetTarget(out var subscriber)) {
                        deadSubscribers.Add(subscriberItem.Key);
                    }
                    else {
                        yield return subscriber;
                    }
                }
            }
            finally {
                _subscriberCollectionLock.ExitReadLock();
            }

            // Remove dead subscribers
            if (deadSubscribers.Count > 0) {
                _subscriberCollectionLock.EnterWriteLock();

                try {
                    foreach (var deadSubscriber in deadSubscribers) {
                        _subscribers.TryRemove(deadSubscriber, out _);
                    }
                }
                finally {
                    _subscriberCollectionLock.ExitWriteLock();
                }
            }
        }

        public void Dispose() {
            _subscriberCollectionLock?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
