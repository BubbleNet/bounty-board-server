using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BountyBoardServer.Entities
{
    public class RefreshToken
    {
       public int Id { get; set; }
        public string Token { get; set; }
        public User User { get; set; }
    }
}
