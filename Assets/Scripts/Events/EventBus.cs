using System;
using System.Collections.Generic;

namespace Game.Events
{
    public sealed class EventBus
    {
        private interface IEventList
        {
            void RemoveOwner(object owner);
            void Clear();
        }

        private sealed class EventList<T> : IEventList where T : struct, IGameEvent
        {
            private struct Listener
            {
                public Action<T> Callback;
                public object Owner;
                public bool IsActive;
            }

            private readonly List<Listener> _listeners = new List<Listener>(8);
            private readonly List<Listener> _pendingAdditions = new List<Listener>(4);
            private bool _hasPendingRemovals;
            private bool _isPublishing;

            public void Add(Action<T> callback, object owner)
            {
                if (callback == null)
                {
                    return;
                }

                if (Contains(_listeners, callback, owner))
                {
                    return;
                }

                if (Contains(_pendingAdditions, callback, owner))
                {
                    return;
                }

                var listener = new Listener
                {
                    Callback = callback,
                    Owner = owner,
                    IsActive = true
                };

                if (_isPublishing)
                {
                    _pendingAdditions.Add(listener);
                    return;
                }

                _listeners.Add(listener);
            }

            public void Remove(Action<T> callback, object owner)
            {
                if (callback == null)
                {
                    return;
                }

                for (int i = 0; i < _listeners.Count; i++)
                {
                    var listener = _listeners[i];
                    if (listener.IsActive && listener.Callback == callback && Equals(listener.Owner, owner))
                    {
                        RemoveAt(i);
                        return;
                    }
                }

                RemoveFromPendingAdditions(callback, owner);
            }

            public void Publish(in T evt)
            {
                _isPublishing = true;

                for (int i = 0; i < _listeners.Count; i++)
                {
                    var listener = _listeners[i];
                    if (!listener.IsActive)
                    {
                        continue;
                    }

                    var callback = listener.Callback;
                    callback?.Invoke(evt);
                }

                _isPublishing = false;
                FlushPendingChanges();
            }

            public void RemoveOwner(object owner)
            {
                if (owner == null)
                {
                    return;
                }

                for (int i = _listeners.Count - 1; i >= 0; i--)
                {
                    if (_listeners[i].IsActive && Equals(_listeners[i].Owner, owner))
                    {
                        RemoveAt(i);
                    }
                }

                for (int i = _pendingAdditions.Count - 1; i >= 0; i--)
                {
                    if (Equals(_pendingAdditions[i].Owner, owner))
                    {
                        _pendingAdditions.RemoveAt(i);
                    }
                }
            }

            public void Clear()
            {
                _listeners.Clear();
                _pendingAdditions.Clear();
                _hasPendingRemovals = false;
                _isPublishing = false;
            }

            private void RemoveAt(int index)
            {
                if (_isPublishing)
                {
                    var listener = _listeners[index];
                    if (!listener.IsActive)
                    {
                        return;
                    }

                    listener.IsActive = false;
                    _listeners[index] = listener;
                    _hasPendingRemovals = true;
                    return;
                }

                _listeners.RemoveAt(index);
            }

            private void FlushPendingChanges()
            {
                if (_hasPendingRemovals)
                {
                    for (int i = _listeners.Count - 1; i >= 0; i--)
                    {
                        if (!_listeners[i].IsActive)
                        {
                            _listeners.RemoveAt(i);
                        }
                    }

                    _hasPendingRemovals = false;
                }

                if (_pendingAdditions.Count == 0)
                {
                    return;
                }

                for (int i = 0; i < _pendingAdditions.Count; i++)
                {
                    var listener = _pendingAdditions[i];
                    if (listener.IsActive && !Contains(_listeners, listener.Callback, listener.Owner))
                    {
                        _listeners.Add(listener);
                    }
                }

                _pendingAdditions.Clear();
            }

            private static bool Contains(List<Listener> listeners, Action<T> callback, object owner)
            {
                for (int i = 0; i < listeners.Count; i++)
                {
                    var listener = listeners[i];
                    if (listener.IsActive && listener.Callback == callback && Equals(listener.Owner, owner))
                    {
                        return true;
                    }
                }

                return false;
            }

            private void RemoveFromPendingAdditions(Action<T> callback, object owner)
            {
                for (int i = _pendingAdditions.Count - 1; i >= 0; i--)
                {
                    var listener = _pendingAdditions[i];
                    if (listener.Callback == callback && Equals(listener.Owner, owner))
                    {
                        _pendingAdditions.RemoveAt(i);
                    }
                }
            }
        }

        private readonly Dictionary<Type, IEventList> _eventLists = new Dictionary<Type, IEventList>(32);

        public static EventBus Global { get; } = new EventBus();

        public EventBinding<T> Subscribe<T>(Action<T> callback, object owner = null) where T : struct, IGameEvent
        {
            var list = GetOrCreateList<T>();
            list.Add(callback, owner);
            return new EventBinding<T>(this, callback, owner);
        }

        public void Unsubscribe<T>(Action<T> callback, object owner = null) where T : struct, IGameEvent
        {
            if (callback == null)
            {
                return;
            }

            if (_eventLists.TryGetValue(typeof(T), out var rawList))
            {
                ((EventList<T>)rawList).Remove(callback, owner);
            }
        }

        public void Publish<T>(in T evt) where T : struct, IGameEvent
        {
            if (_eventLists.TryGetValue(typeof(T), out var rawList))
            {
                ((EventList<T>)rawList).Publish(evt);
            }
        }

        public void RemoveOwner(object owner)
        {
            if (owner == null)
            {
                return;
            }

            foreach (var eventList in _eventLists.Values)
            {
                eventList.RemoveOwner(owner);
            }
        }

        public void Clear()
        {
            foreach (var eventList in _eventLists.Values)
            {
                eventList.Clear();
            }

            _eventLists.Clear();
        }

        private EventList<T> GetOrCreateList<T>() where T : struct, IGameEvent
        {
            var eventType = typeof(T);
            if (_eventLists.TryGetValue(eventType, out var rawList))
            {
                return (EventList<T>)rawList;
            }

            var list = new EventList<T>();
            _eventLists.Add(eventType, list);
            return list;
        }
    }
}
