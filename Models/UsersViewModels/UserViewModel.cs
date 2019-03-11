using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TangyTomato.Models.UsersViewModels
{
    public class UserViewModel
    {
        public ApplicationUser ApplicationUser { get; set; }
        public string LockoutEndString { get; set; }
    }
}
