using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Newtonsoft.Json;
using TodoList.Events;

namespace TodoList
{
    public class TodoList : IDisposable
    {
        private SqlConnection _sqlConnection;

        public TodoList()
        {
            _sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TodoListDb"].ConnectionString);
            _sqlConnection.Open();
        }

        public void Dispose()
        {
            _sqlConnection.Dispose();
            _sqlConnection = null;
        }

        public TodoItem Add(string name)
        {
            var item = new TodoItem(Guid.NewGuid());
            AttachEventListeners(item);
            item.ChangeName(name);

            return item;
        }

        public TodoItem Get(Guid id)
        {
            var command = _sqlConnection.CreateCommand();
            command.CommandText = @"SELECT [Version], [EventName], [EventData] FROM [TodoItem] WHERE [Id] = @id";
            command.Parameters.AddWithValue("@id", id);

            var events = new List<IVersionedEvent>();
            var eventLocation = typeof (VersionedEvent).Namespace;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var eventData = reader.GetString(reader.GetOrdinal("EventData"));
                    var typeName = reader.GetString(reader.GetOrdinal("EventName"));
                    var eventType = Type.GetType(string.Join(".", eventLocation, typeName));
                    var @event = (IVersionedEvent)JsonConvert.DeserializeObject(eventData, eventType);
                    events.Add(@event);
                }
            }

            if (events.Count == 0) return null;

            var todoItem = new TodoItem(id, events.OrderBy(e => e.Version));

            AttachEventListeners(todoItem);

            return todoItem;
        }

        private void AttachEventListeners(TodoItem item)
        {
            item.Events
                .Subscribe(WriteEvent);

            item.Events
                .Subscribe(e => WriteTodoListItemView(e, item));
        }

        private void WriteEvent(IVersionedEvent e)
        {
            var command = _sqlConnection.CreateCommand();
            command.CommandText = @"INSERT INTO [TodoItem] ([Id], [Version], [EventName], [EventData]) VALUES (@id, @version, @eventName, @eventData)";
            command.Parameters.AddWithValue("@id", e.SourceId);
            command.Parameters.AddWithValue("@version", e.Version);
            command.Parameters.AddWithValue("@eventName", e.GetType().Name);
            command.Parameters.AddWithValue("@eventData", JsonConvert.SerializeObject(e));
            command.ExecuteNonQuery();
        }
        private void WriteTodoListItemView(IVersionedEvent e, TodoItem item)
        {
            var command = _sqlConnection.CreateCommand();
            command.CommandText = @"MERGE [TodoListItem] AS Target
                                    USING (SELECT @id AS [Id], @name AS [Name], @done AS [Done]) AS Source
                                        ON Target.Id = Source.Id
                                    WHEN NOT MATCHED THEN
                                        INSERT([Id], [Name], [Done]) VALUES(Source.[Id], Source.[Name], Source.[Done])
                                    WHEN MATCHED THEN
                                        UPDATE SET Target.[Name] = Source.[Name], Target.[Done] = Source.[Done];";
            command.Parameters.AddWithValue("@id", item.Id);
            command.Parameters.AddWithValue("@name", item.Name);
            command.Parameters.AddWithValue("@done", item.Done);
            command.ExecuteNonQuery();
        }
    }
}