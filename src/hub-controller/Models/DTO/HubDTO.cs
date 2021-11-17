using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Models.DTO
{
    public class HubDTO
    {
        public String Name { get; set; }
        public Guid HubId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
