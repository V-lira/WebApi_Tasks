using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication7.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public bool IsArchived { get; set; }
    }
}