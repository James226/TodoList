namespace TodoList.Events
{
    public class DoneEvent : VersionedEvent
    {
        public string Username { get; set; }
    }
}