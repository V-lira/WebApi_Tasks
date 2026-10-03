using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApplication7.Data;
using WebApplication7.Models;

namespace WebApplication7.Controllers
{
    [RoutePrefix("api/projects")]
    public class ProjectsController : ApiController
    {
        //api/projects
        [HttpGet, Route("")]
        public IEnumerable<Project> GetAll()
        {
            return AppData.Projects;
        }

        ///api/projects/{id}
        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            var project = AppData.Projects.FirstOrDefault(p => p.Id == id);
            if (project == null)
                return Content(HttpStatusCode.NotFound, new { message = "нету такого, Увы" });
            return Ok(project);
        }

        // /api/projects
        [HttpPost, Route("")]
        public IHttpActionResult Create([FromBody] Project request)
        {
            if (request == null)
                return BadRequest("пусто");

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Имя почему пустое?");

            if (string.IsNullOrWhiteSpace(request.Owner))
                return BadRequest("владельца нет?");

            var name = request.Name.Trim();
            var dup = AppData.Projects.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

            if (dup)
                return Content(HttpStatusCode.Conflict, new { message = "уже есть такой" });

            var project = new Project
            {
                Id = AppData.NextProjectId(),
                Name = name,
                Owner = request.Owner.Trim(),
                IsArchived = false
            };

            AppData.Projects.Add(project);

            return Content(HttpStatusCode.Created, project);
        }

        //api/projects/{id}/archive
        [HttpPut, Route("{id:int}/archive")]
        public IHttpActionResult Archive(int id)
        {
            var project = AppData.Projects.FirstOrDefault(p => p.Id == id);
            if (project == null)
                return Content(HttpStatusCode.NotFound, new { message = "нету такого!" });

            if (project.IsArchived)
                return Content(HttpStatusCode.Conflict, new { message = "проект уже давно архивирован" });

            project.IsArchived = true;
            return StatusCode(HttpStatusCode.NoContent);
        }

        //api/projects/{projectId}/tasks
        [HttpGet, Route("{projectId:int}/tasks")]
        public IHttpActionResult GetProjectTasks(int projectId)
        {
            var project = AppData.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
                return Content(HttpStatusCode.NotFound, new { message = "нету такого!" });

            var tasks = AppData.Tasks.Where(t => t.ProjectId == projectId).ToList();
            return Ok(tasks);
        }

        //api/projects/{id}/stats
        [HttpGet, Route("{id:int}/stats")]
        public IHttpActionResult Stats(int id)
        {
            var project = AppData.Projects.FirstOrDefault(p => p.Id == id);
            if (project == null)
                return Content(HttpStatusCode.NotFound, new { message = "Проект не найден" });

            var tasks = AppData.Tasks.Where(t => t.ProjectId == id).ToList();
            var today = DateTime.Today;

            var stats = new
            {
                projectId = project.Id,
                projectName = project.Name,
                totalTasks = tasks.Count,
                newTasks = tasks.Count(t => t.Status == "New"),
                inProgressTasks = tasks.Count(t => t.Status == "InProgress"),
                doneTasks = tasks.Count(t => t.Status == "Done"),
                cancelledTasks = tasks.Count(t => t.Status == "Cancelled"),
                overdueTasks = tasks.Count(t =>
                    t.DueDate.HasValue &&
                    t.DueDate.Value < today &&
                    t.Status != "Done" &&
                    t.Status != "Cancelled")
            };

            return Ok(stats);
        }
    }
}