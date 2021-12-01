using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Models.DTO
{
    public class HubDTO
    {
        public Guid HubId { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ThingDTO> Things { get; set; }
    }
}
