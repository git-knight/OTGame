using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TGame.Entities
{
    public class User : IdentityUser
    {
        public Hero Hero { get; set; }
    }
}
