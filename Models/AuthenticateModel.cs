using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BountyBoardServer.Model
{
    public class AuthenticateModel
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
