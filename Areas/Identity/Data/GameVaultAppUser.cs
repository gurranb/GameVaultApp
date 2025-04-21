using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace GameVaultApp.Areas.Identity.Data;

// Add profile data for application users by adding properties to the GameVaultAppUser class
public class GameVaultAppUser : IdentityUser
{
    [PersonalData]
    public string FirstName { get; set; }

    [PersonalData]
    public string LastName { get; set; }

}

