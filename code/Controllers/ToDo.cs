using System;

namespace todo_app.Controllers
{
    public class ToDo{
        public int Id { get; set; }
        public DateTime CreatedTime  {get;set;}
        public string Status {get;set;}
        public string Task {get;set;}
    }
}