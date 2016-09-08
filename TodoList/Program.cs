using System;

namespace TodoList
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var id = Guid.NewGuid();

            using (var list = new TodoList())
            {
                var item = list.Add("Stack the shelves!");
                Console.WriteLine($"{item.Name}: {item.Done}");

                item.MarkDone(string.Empty);

                Console.WriteLine($"{item.Name}: {item.Done}");

                var newItem = list.Get(item.Id);
                Console.WriteLine($"{newItem.Name}: {newItem.Done}");
                Console.ReadLine();
            }
        }
    }
}
