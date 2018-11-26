using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ContactService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        /// <summary>
        /// Get all values from project service.
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
            => Ok(Enumerable.Range(0, 8).Select(p => $"Contact_{p}"));
    }
}
