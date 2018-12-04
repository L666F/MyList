using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyList.Models;
using MyList.Models.Data;
using MyList.Models.ViewModels;

namespace MyList.Controllers
{
    [Produces("application/json")]
    [Route("api/User")]
    public class UserController : Controller
    {
        private readonly IConfiguration config;
        private readonly ApplicationDbContext context;

        public UserController(IConfiguration config, ApplicationDbContext context)
        {
            this.config = config;
            this.context = context;
        }

        private string BuildToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim("ID",user.ID+""),
                new Claim("Username",user.Username),
            };

            var token = new JwtSecurityToken(config["Jwt:Issuer"],
              config["Jwt:Issuer"],
              claims: claims,
              expires: DateTime.Now.AddMinutes(30),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private Boolean UsernameIsUnique(string username)
        {
            if(context.Users.SingleOrDefault(m => m.Username == username) != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private class SuccessPacket
        {
            public string Token { get; set; }
            public string Username { get; set; }
            public int ID { get; set; }
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody]RegisterVM vm)
        {
            if (ModelState.IsValid)
            {
                //USERNAME UNIQUE
                if (!UsernameIsUnique(vm.Username))
                    return BadRequest("The username you chose is already in use.");

                //PASSWORD REQUIREMENTS
                if (!vm.Password.Any(char.IsLower))
                    return BadRequest("The password should contain at least 1 lowercase letter.");
                if (!vm.Password.Any(char.IsUpper))
                    return BadRequest("The password should contain at least 1 uppercase letter.");
                if (!vm.Password.Any(char.IsNumber))
                    return BadRequest("The password should contain at least 1 number.");
                if (!vm.Password.Any(char.IsSymbol))
                    return BadRequest("The password should contain at least 1 symbol.");

                //ADDING NEW USER TO DB
                var userToRegister = new ApplicationUser()
                {
                    Username = vm.Username,
                    PasswordHash = SecurePasswordHasher.Hash(vm.Password),
                    FullName = vm.FullName
                };
                await context.Users.AddAsync(userToRegister);
                await context.SaveChangesAsync();

                var registeredUser = context.Users.Single(m => m.Username == userToRegister.Username);
                return Ok(new SuccessPacket() { Username = registeredUser.Username, ID = registeredUser.ID, Token = BuildToken(registeredUser) });
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody]LoginVM vm)
        {
            if (ModelState.IsValid)
            {
                var userInDb = context.Users.SingleOrDefault(m => m.Username == vm.Username);
                if (userInDb == null)
                    return NotFound("User not found.");

                if (SecurePasswordHasher.Verify(vm.Password, userInDb.PasswordHash))
                {
                    return Ok(new SuccessPacket() { Username = userInDb.Username, ID = userInDb.ID, Token = BuildToken(userInDb) });
                }
                else
                {
                    return Unauthorized();
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [Authorize]
        [HttpPost("changepassword")]
        public async Task<ActionResult> ChangePassword([FromBody]ChangePasswordVM vm)
        {
            if (ModelState.IsValid)
            {
                //PASSWORD REQUIREMENTS
                if (!vm.NewPassword.Any(char.IsLower))
                    return BadRequest("The password should contain at least 1 lowercase letter.");
                if (!vm.NewPassword.Any(char.IsUpper))
                    return BadRequest("The password should contain at least 1 uppercase letter.");
                if (!vm.NewPassword.Any(char.IsNumber))
                    return BadRequest("The password should contain at least 1 number.");
                if (!vm.NewPassword.Any(char.IsSymbol) && !vm.NewPassword.Any(char.IsPunctuation))
                    return BadRequest("The password should contain at least 1 non-alphanumeric character.");

                var user = HttpContext.User;
                if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                    return Forbid();

                var userInDb = await context.Users.FindAsync(ID);
                if (userInDb == null)
                    return NotFound();

                if (!SecurePasswordHasher.Verify(vm.PreviousPassword, userInDb.PasswordHash))
                    return Unauthorized();

                userInDb.PasswordHash = SecurePasswordHasher.Hash(vm.NewPassword);
                await context.SaveChangesAsync();

                return Ok();

            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpDelete("delete")]
        [Authorize]
        public async Task<ActionResult> DeleteAccount()
        {
            var user = HttpContext.User;
            int ID = 0;
            if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out ID))
                return Forbid();

            var userInDb = await context.Users.FindAsync(ID);
            if (userInDb == null)
                return NotFound();


            if (user.HasClaim(c => c.Type == "FullName"))
            {
                if(userInDb.FullName==user.Claims.FirstOrDefault(c => c.Type == "FullName").Value)
                {
                    //DELETE USER
                    context.Invites.RemoveRange(context.Invites.Where(m => m.InviterID == ID || m.InvitedID == ID));
                    context.UserLists.RemoveRange(context.UserLists.Where(m => m.UserID == ID));
                    context.Users.Remove(userInDb);
                    await context.SaveChangesAsync();
                    return Ok();
                }
                else
                {
                    return Forbid();
                }                    
            }
            else
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var claims = new[]
                {
                    new Claim("FullName",userInDb.FullName),
                    new Claim("ID",userInDb.ID+"")
                };

                var token = new JwtSecurityToken(config["Jwt:Issuer"],
                  config["Jwt:Issuer"],
                  claims: claims,
                  expires: DateTime.Now.AddMinutes(1),
                  signingCredentials: creds);

                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }
        }

        private class UserListToReturn
        {
            public List<UserToReturn> Users { get; set; }

            public class UserToReturn
            {
                public string Username { get; set; }
            }
        }

        [HttpGet("get")]
        [Authorize]
        public ActionResult GetUsers()
        {
            var user = HttpContext.User;
            int ID = 0;
            if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out ID))
                return Forbid();

            var users = context.Users.Where(m => m.ID != ID);
            var usersToReturn = new List<UserListToReturn.UserToReturn>();

            foreach(ApplicationUser u in users)
            {
                usersToReturn.Add(new UserListToReturn.UserToReturn() { Username = u.Username });
            }

            return Ok(usersToReturn);
        }

        private class UserToReturnSingle
        {
            public string Username { get; set; }
            public string FullName { get; set; }
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public async Task<ActionResult> GetUser(int? id)
        {
            var user = HttpContext.User;
            int ID = 0;
            if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out ID))
                return Forbid();

            if (id != null)
            {
                var userInDb = await context.Users.FindAsync(id);
                if (userInDb == null)
                    return NotFound();

                if (userInDb.ID == ID)
                    return Ok(new UserToReturnSingle() { Username = userInDb.Username, FullName = userInDb.FullName });
                else
                    return Ok(new UserListToReturn.UserToReturn() { Username = userInDb.Username });
            }
            
            var u = await context.Users.FindAsync(ID);
            if (u == null)
                return NotFound();

            return Ok(new UserToReturnSingle() { Username = u.Username, FullName = u.FullName });

        }
    }
}