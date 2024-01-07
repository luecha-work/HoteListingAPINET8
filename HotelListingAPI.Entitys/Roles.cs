using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace HotelListingAPI.Entitys
{
    public class Roles : IdentityRole
    {
        public string RoleCode { get; set; }
        public DateTimeOffset Create_At { get; set; }
        public DateTimeOffset Update_At { get; set; }
        public string Update_By { get; set; }
        public string Create_By { get; set; }
    }
}
