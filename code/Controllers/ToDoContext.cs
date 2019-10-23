using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace todo_app.Controllers
{
    public class ToDoContext : DbContext
   {
      string TODO_CONNECTION_STRING = "";
      
      public ToDoContext(IOptions<Parameters> options)
      {
         Console.WriteLine("DB Connection String ToDoContext - " + options.Value.AuroraConnectionString);
         TODO_CONNECTION_STRING = options.Value.AuroraConnectionString;
      }
      

      protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
      {
         optionsBuilder.UseMySQL(TODO_CONNECTION_STRING);
         base.OnConfiguring(optionsBuilder);
      }
      public DbSet<ToDo> ToDos { get; set; }
   }
}