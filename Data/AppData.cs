using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication7.Models;

namespace WebApplication7.Data
{
    public static class AppData
    {
        public static List<Project> Projects { get; set; }
        public static List<TaskItem> Tasks { get; set; }

        private static int _nextProjectId = 1;
        private static int _nextTaskId = 1;

        static AppData()
        {
            Projects = new List<Project>();
            Tasks = new List<TaskItem>();
            Seed();
        }

        public static int NextProjectId() => _nextProjectId++;
        public static int NextTaskId() => _nextTaskId++;

        private static void Seed()
        {
            //3 надо
            Projects.Add(new Project { Id = NextProjectId(), Name = "Новый сайт", Owner = "Иванов И.И.", IsArchived = false });
            Projects.Add(new Project { Id = NextProjectId(), Name = "Название", Owner = "Петров П..П.", IsArchived = false });
            Projects.Add(new Project { Id = NextProjectId(), Name = "Новое что-то", Owner = "Ванечкин И.И.", IsArchived = true });

            var today = DateTime.Today;

            //8 разных:
            Tasks.Add(new TaskItem
            {
                Id = NextTaskId(),
                ProjectId = 1,
                Title = "создать страницу входа",
                Description = "добавить форму авторизации и проверку данных",
                Status = "New",
                Priority = 3,
                CreatedAt = today.AddDays(-2),
                DueDate = today.AddDays(3)
            });

            Tasks.Add(new TaskItem
            {
                Id = NextTaskId(),
                ProjectId = 1,
                Title = "свёрстать главную",
                Description = "сделать адаптивную вёрстку главной страницы",
                Status = "InProgress",
                Priority = 4,
                CreatedAt = today.AddDays(-5),
                DueDate = today.AddDays(-1)   //<<<---- она просрочена
            });

            Tasks.Add(new TaskItem
            {
                Id = NextTaskId(),
                ProjectId = 1,
                Title = "Настроить общение",
                Description = "софт скилы самое то в рабочее время",
                Status = "Done",
                Priority = 2,
                CreatedAt = today.AddDays(-10),
                DueDate = today.AddDays(-5)
            });

            Tasks.Add(new TaskItem
            {
                Id = NextTaskId(),
                ProjectId = 1,
                Title = "удалить мусорку на компе",
                Description = "сломай систему",
                Status = "Cancelled",
                Priority = 1,
                CreatedAt = today.AddDays(-7),
                DueDate = null
            });

            Tasks.Add(new TaskItem
            {
                Id = NextTaskId(),
                ProjectId = 2,
                Title = "экран включить",
                Description = "делать вид, что работаешь",
                Status = "New",
                Priority = 3,
                CreatedAt = today.AddDays(-1),
                DueDate = today.AddDays(5)
            });

            Tasks.Add(new TaskItem
            {
                Id = NextTaskId(),
                ProjectId = 2,
                Title = "чекнуть бд",
                Description = "поработать над бд",
                Status = "InProgress",
                Priority = 4,
                CreatedAt = today.AddDays(-4),
                DueDate = today.AddDays(2)
            });

            Tasks.Add(new TaskItem
            {
                Id = NextTaskId(),
                ProjectId = 2,
                Title = "тестирование",
                Description = "заставить тестировщика работать",
                Status = "Done",
                Priority = 2,
                CreatedAt = today.AddDays(-15),
                DueDate = today.AddDays(-10)
            });

            Tasks.Add(new TaskItem
            {
                Id = NextTaskId(),
                ProjectId = 2,
                Title = "подготовить релиз",
                Description = "собрать релизную сборку и загрузить куда надо",
                Status = "New",
                Priority = 4,
                CreatedAt = today.AddDays(-1),
                DueDate = today.AddDays(-3)   //<<<---- она просрочена
            });
        }
    }
}