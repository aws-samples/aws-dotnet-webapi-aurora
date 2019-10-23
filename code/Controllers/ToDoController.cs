using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace todo_app.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToDoController : ControllerBase
    {
        private IOptions<Parameters> _options;
        public ToDoController(IOptions<Parameters> options)
        {
            Console.WriteLine("ToDo Controller!");
            _options = options;
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            StringBuilder response = new StringBuilder();
            using (var db = new ToDoContext(_options))
            {
                foreach(ToDo tdo in db.ToDos)
                {
                    response.Append(tdo.Id + "  " + tdo.CreatedTime + " " + tdo.Status + " " + tdo.Task + "      ");
                }
            }
            return new string[] { "value from db - ", response.ToString()};
        }

        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post(ToDo todoVal)
        {
            Console.WriteLine("Entering ToDo Put - " + JsonConvert.SerializeObject(todoVal, Formatting.Indented));            
            
            if (todoVal != null && !string.IsNullOrEmpty(todoVal.Status) && 
                    !string.IsNullOrEmpty(todoVal.Task)){
                Console.WriteLine("Saving DBContext!");
                using (var db = new ToDoContext(_options))
                {
                    todoVal.CreatedTime = DateTime.Now;
                    var todo = db.ToDos.Add(todoVal).Entity;
                    Console.WriteLine(todo?.Task);
                    db.SaveChanges();
                }
            }

            Console.WriteLine("ToDo Saved Succesfully!");
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
            
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
