using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using TodoList.Events;

namespace TodoList
{
    public abstract class Aggregate : IAggregate
    {
        private readonly Dictionary<Type, Action<IVersionedEvent>> handlers = new Dictionary<Type, Action<IVersionedEvent>>();
        private readonly Subject<IVersionedEvent> pendingEvents = new Subject<IVersionedEvent>();

        private readonly Guid id;
        private int version = -1;

        protected Aggregate(Guid id)
        {
            this.id = id;
        }

        public Guid Id
        {
            get { return id; }
        }

        /// <summary>
        /// Gets the entity's version. As the entity is being updated and events being generated, the version is incremented.
        /// </summary>
        public int Version
        {
            get { return version; }
            protected set { version = value; }
        }

        /// <summary>
        /// Gets the collection of new events since the entity was loaded, as a consequence of command handling.
        /// </summary>
        public IObservable<IVersionedEvent> Events => pendingEvents;

        /// <summary>
        /// Configures a handler for an event. 
        /// </summary>
        protected void Handles<TEvent>(Action<TEvent> handler)
            where TEvent : IEvent
        {
            handlers.Add(typeof(TEvent), @event => handler((TEvent)@event));
        }

        protected void LoadFrom(IEnumerable<IVersionedEvent> pastEvents)
        {
            foreach (var e in pastEvents)
            {
                handlers[e.GetType()].Invoke(e);
                version = e.Version;
            }
        }

        protected void Update(VersionedEvent e)
        {
            e.SourceId = Id;
            e.Version = version + 1;
            handlers[e.GetType()].Invoke(e);
            version = e.Version;
            pendingEvents.OnNext(e);
        }
    }
}