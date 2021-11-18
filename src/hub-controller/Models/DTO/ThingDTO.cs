using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Models.DTO
{
    public class ThingDTO
    {
        public String ThingId { get; set; }
        public String Name { get; set; }
        public String Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public String Description { get; set; }
    }
}
