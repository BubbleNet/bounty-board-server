using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel;
using Microsoft.IdentityModel.Tokens;

namespace BountyBoardServer.Entities
{
    public class TokenModel
    {
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiration { get; set;  }
        public string RefreshToken { get; set; }

    }
}

    
