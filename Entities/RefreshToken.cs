using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BountyBoardServer.Entities
{
    public class RefreshToken
    {
       public int Id { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        public User User { get; set; }
    }
}
