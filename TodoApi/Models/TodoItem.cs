using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace TodoApi.Models
{
    public class TodoItem
    {
        public long? Id { get; set; }
        [BindProperty]
        public string Name { get; set; }
       [BindProperty] 
        public bool IsComplete { get; set; }

        public TodoItem(object[] fields)
        {
            Console.WriteLine(String.Join(",", fields));
            Id = (long)fields[0];
            Name = (string)fields[1];
            IsComplete = (bool)fields[2];
        }
        
        [JsonConstructor]
        public TodoItem() {}

        public override string ToString()
        {
            return $"{Id}, {Name}, {IsComplete}";
        }
    }
}
