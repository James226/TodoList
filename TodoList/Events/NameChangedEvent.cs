namespace TodoList.Events
{
    public class NameChangedEvent : VersionedEvent
    {
        public string Name { get; set; }
    }
}