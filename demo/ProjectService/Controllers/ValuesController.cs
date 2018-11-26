using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace ProjectService.Controllers
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
            => Ok(Enumerable.Range(0, 8).Select(p => $"Project_{p}"));
    }
}
