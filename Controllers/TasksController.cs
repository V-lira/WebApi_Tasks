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
    [RoutePrefix("api/tasks")]
    public class TasksController : ApiController
    {
        private static readonly string[] AllowedStatuses ={ "New", "InProgress", "Done", "Cancelled" };
        ///api/tasks?status=&priority=&projectId=&search=
        [HttpGet, Route("")]
        public IHttpActionResult GetAll(
            [FromUri] string status = null,
            [FromUri] int? priority = null,
            [FromUri] int? projectId = null,
            [FromUri] string search = null)
        {
            var query = AppData.Tasks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!AllowedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                    return BadRequest("странный статус: " + status);

                var s = status.Trim();
                query = query.Where(t => string.Equals(t.Status, s, StringComparison.OrdinalIgnoreCase));
            }

            if (priority.HasValue)
            {
                if (priority.Value < 1 || priority.Value > 4)
                    return BadRequest("приоритет тут должен быть от 1 до 4!!!");
                query = query.Where(t => t.Priority == priority.Value);
            }

            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLowerInvariant();
                query = query.Where(t => (t.Title != null && t.Title.ToLower().Contains(q)) || (t.Description != null && t.Description.ToLower().Contains(q)));
            }
            return Ok(query.ToList());
        }

        //ЗАМЕТОЧКА ВАЖНАЯ: overdue должен идти ДО!!! {id:int}, иначе не смэтчится :(
        [HttpGet, Route("overdue")]
        public IEnumerable<TaskItem> Overdue()
        {
            var today = DateTime.Today;
            return AppData.Tasks
                .Where(t =>
                    t.DueDate.HasValue &&
                    t.DueDate.Value < today &&
                    t.Status != "Done" &&
                    t.Status != "Cancelled")
                .ToList();
        }

        ///api/tasks/{id}
        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            var task = AppData.Tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return Content(HttpStatusCode.NotFound, new { message = "такого тут нет" });
            return Ok(task);
        }

        ///api/tasks
        [HttpPost, Route("")]
        public IHttpActionResult Create([FromBody] TaskItem request)
        {
            if (request == null)
                return BadRequest("Тело запроса пустое");

            var project = AppData.Projects.FirstOrDefault(p => p.Id == request.ProjectId);
            if (project == null)
                return Content(HttpStatusCode.NotFound, new { message = "такого у нас тут нет" });

            if (project.IsArchived)
                return Content(HttpStatusCode.Conflict,
                    new { message = "АЙ-АЙ-АЙ! НЕЛЬЗЯ добавить задачу в АРХИВИРОВАННЫЙ проект" });

            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest("почему название пустое?");

            if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Trim().Length < 10)
                return BadRequest("минимум 10 символов");

            if (request.Priority < 1 || request.Priority > 4)
                return BadRequest("приоритет должен быть от 1 до 4");

            if (request.DueDate.HasValue && request.DueDate.Value.Date < DateTime.Today)
                return BadRequest("DueDate не может быть раньше текущей даты");

            var task = new TaskItem
            {
                Id = AppData.NextTaskId(),
                ProjectId = request.ProjectId,
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                Status = "New",
                Priority = request.Priority,
                CreatedAt = DateTime.Now,
                DueDate = request.DueDate
            };

            AppData.Tasks.Add(task);
            return Content(HttpStatusCode.Created, task);
        }

        //api/tasks/{id}
        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] TaskItem request)
        {
            var task = AppData.Tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return Content(HttpStatusCode.NotFound, new { message = "такого нет у нас" });

            if (request == null)
                return BadRequest("пусто");

            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest("пустое название...");

            if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Trim().Length < 10)
                return BadRequest("минимум 10 символов");

            if (request.Priority < 1 || request.Priority > 4)
                return BadRequest("приоритет должен быть от 1 до 4");

            if (request.DueDate.HasValue && request.DueDate.Value.Date < DateTime.Today)
                return BadRequest("DueDate не может быть раньше текущей даты");

            task.Title = request.Title.Trim();
            task.Description = request.Description.Trim();
            task.Priority = request.Priority;
            task.DueDate = request.DueDate;
            return Ok(task);
        }

        // /api/tasks/{id}/status
        [HttpPut, Route("{id:int}/status")]
        public IHttpActionResult ChangeStatus(int id, [FromBody] dynamic body)
        {
            var task = AppData.Tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return Content(HttpStatusCode.NotFound, new { message = "нету такого здесь" });

            string newStatus = body?.status;
            if (string.IsNullOrWhiteSpace(newStatus))
                return BadRequest("status обязательно");

            if (!AllowedStatuses.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
                return BadRequest("что за статус: " + newStatus);

            newStatus = AllowedStatuses.First(s => string.Equals(s, newStatus, StringComparison.OrdinalIgnoreCase));

            if (!IsTransitionAllowed(task.Status, newStatus))
                return Content(HttpStatusCode.Conflict,
                    new { message = "переход " + task.Status + " -> " + newStatus + " запрещён" });

            task.Status = newStatus;
            return Ok(task);
        }

        // /api/tasks/{id}
        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            var task = AppData.Tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return Content(HttpStatusCode.NotFound, new { message = "нету такого!!!" });

            if (task.Status == "InProgress")
                return Content(HttpStatusCode.Conflict,
                    new { message = "ай-яй-яй! нельзя удалить задачу, которая находится в работе" });

            AppData.Tasks.Remove(task);
            return StatusCode(HttpStatusCode.NoContent);
        }

        private static bool IsTransitionAllowed(string from, string to)
        {
            switch (from)
            {
                case "New":
                    return to == "InProgress" || to == "Cancelled";
                case "InProgress":
                    return to == "Done" || to == "Cancelled";
                default:
                    return false;
            }
        }
    }
}
