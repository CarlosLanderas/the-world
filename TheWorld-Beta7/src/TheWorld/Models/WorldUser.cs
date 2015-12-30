using System;
using Microsoft.AspNet.Identity.EntityFramework;

namespace TheWorld.Models
{
    public class WorldUser : IdentityUser
    {
        public DateTime FirstStrip { get; set; }
    }
}