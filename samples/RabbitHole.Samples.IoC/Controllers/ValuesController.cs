using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RabbitHole.Samples.IoC.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        IDebitBus _bus;
        public ValuesController(IDebitBus bus)
        {
            _bus = bus;
        }
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            _bus.Publish(new DebitAccountCommand
            {
                AccountNumber = "1223454-34",
                Amouunt = 120
            });
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
