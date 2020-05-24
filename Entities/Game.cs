using BountyBoardServer.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BountyBoardServer.Entities
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Edition> Editions { get; set; }
        public string Publisher { get; set; }
        public string Description { get; set; }

        public BasicGameDto ToBasicGameDto()
        {
            return new BasicGameDto 
            {
                Id = this.Id,
                Name = this.Name,
                Publisher = this.Publisher,
                Description = this.Description
            };

        }
    }
}
