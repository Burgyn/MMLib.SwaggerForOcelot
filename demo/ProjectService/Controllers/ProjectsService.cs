using Microsoft.AspNetCore.Mvc;
using MMLib.RapidPrototyping.Generators;
using ProjectService.Dto;
using ProjectService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly PersonGenerator _personGenerator = new PersonGenerator();
        private readonly WordGenerator _wordGenerator = new WordGenerator();
        private readonly Lazy<List<Project>> _projects = new Lazy<List<Project>>();

        public ProjectsController()
        {
            int id = 0;

            _projects = new Lazy<List<Project>>(() => new List<Project>(Enumerable.Range(0, 10)
                .Select(p => new Project()
                {
                    Id = ++id,
                    Name = string.Join(" ", _wordGenerator.Next(2)),
                    Description = string.Join(" ", _wordGenerator.Next(5)),
                    Owner = _personGenerator.Next().Mail
                })));
        }

        /// <summary>
        /// Get all Projects.
        /// </summary>
        [ProducesResponseType(200)]
        [HttpGet]
        public ActionResult<IEnumerable<Project>> Get()
            => Ok(_projects.Value);

        /// <summary>
        /// Gets project by id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <response code="404">If project with id doesn't exist.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(200, Type = typeof(Project))]
        [ProducesResponseType(404)]
        public IActionResult Get(int id)
        {
            Project project = _projects.Value.FirstOrDefault(p => p.Id == id);

            if (project != null)
            {
                return Ok(project);
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Creates new project.
        /// </summary>
        /// <param name="projectViewModel">Project creation view model.</param>
        /// <returns>New project id.</returns>
        /// <response code="201">Returns the newly created project.</response>
        /// <response code="400">If validation of <paramref name="projectViewModel"/> failed.</response>
        [HttpPost("projectCreate")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult CreateProject([FromBody] ProjectViewModel projectViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else
            {
                var project = new Project()
                {
                    Id = _projects.Value.Max(p => p.Id) + 1,
                    Name = projectViewModel.Name,
                    Description = projectViewModel.Description,
                    Owner = projectViewModel.Owner
                };
                _projects.Value.Add(project);

                return Created("", new { project.Id });
            }
        }
    }
}
