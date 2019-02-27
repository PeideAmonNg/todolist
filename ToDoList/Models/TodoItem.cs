using Microsoft.AspNetCore.Identity;
using System;
using ToDoList.Models;

namespace TodoApi.Models
{
    public class TodoItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }

        public IdentityUser User { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
