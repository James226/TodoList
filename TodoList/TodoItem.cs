using System;
using System.Collections.Generic;
using TodoList.Events;

namespace TodoList
{
    public class TodoItem : Aggregate
    {
        public string Name { get; set; }
        public bool Done { get; private set; }

        public TodoItem(Guid id) : base(id)
        {
            AddHandlers();
        }

        public TodoItem(Guid id, IEnumerable<IVersionedEvent> events) : base(id)
        {
            AddHandlers();
            LoadFrom(events);
        }

        private void AddHandlers()
        {
            Handles<NameChangedEvent>(Handle);
            Handles<DoneEvent>(Handle);
        }

        private void Handle(NameChangedEvent e)
        {
            Name = e.Name;
        }

        private void Handle(DoneEvent e)
        {
            Done = true;
        }

        public void ChangeName(string name)
        {
            Update(new NameChangedEvent {Name = name});
        }

        public void MarkDone(string username)
        {
            Update(new DoneEvent { Username = username });
        }
    }
}