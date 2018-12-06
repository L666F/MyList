using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyList.Models;
using MyList.Models.Data;
using MyList.Models.VMs;

namespace MyList.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IConfiguration config;

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            this.context = context;
            this.config = config;
        }

        [HttpPost("[action]")]
        public ActionResult Register([FromBody]RegisterVM vm)
        {
            //Validation
            var valObj = new ValidationObject()
            {
                Fields = new List<ValidationObject.FieldValidation>() 
            };

            //Names
            if (vm.FirstName == null)
                valObj.Fields.Add(new ValidationObject.FieldValidation() { Field = "FirstName", ErrorMessages = new List<string>() { "This field is required." } });
            if (vm.LastName == null)
                valObj.Fields.Add(new ValidationObject.FieldValidation() { Field = "LastName", ErrorMessages = new List<string>() { "This field is required." } });

            //Email (valid, unique)
            if(vm.Email == null)
                valObj.Fields.Add(new ValidationObject.FieldValidation() { Field = "Email", ErrorMessages = new List<string>() { "This field is required." } });
            else
            {
                var attr = new EmailAddressAttribute();
                if (!attr.IsValid(vm.Email))
                    valObj.Fields.Add(new ValidationObject.FieldValidation() { Field = "Email", ErrorMessages = new List<string>() { "The email address you entered is invalid." } });
                if (context.Users.Where(m => m.Email == vm.Email).Count() > 0)
                    valObj.Fields.Add(new ValidationObject.FieldValidation() { Field = "Email", ErrorMessages = new List<string>() { "This email address is already taken." } });
            }

            //Password (1+ lowercase, 1+ uppercase, 1+ number, 8 to 50 in length)
            if (vm.Password == null)
                valObj.Fields.Add(new ValidationObject.FieldValidation() { Field = "Password", ErrorMessages = new List<string>() { "This field is required." } });
            else
            {
                if (!vm.Password.Any(Char.IsLower) || !vm.Password.Any(Char.IsUpper))
                    valObj.Fields.Add(new ValidationObject.FieldValidation() { Field = "Password", ErrorMessages = new List<string>() { "The password should contain at least an uppercase character as well as a lowercase one." } });
                if (!vm.Password.Any(Char.IsNumber))
                    valObj.Fields.Add(new ValidationObject.FieldValidation() { Field = "Password", ErrorMessages = new List<string>() { "The password should contain at least a number." } });
                if (vm.Password.Length < 8 || vm.Password.Length > 50)
                    valObj.Fields.Add(new ValidationObject.FieldValidation() { Field = "Password", ErrorMessages = new List<string>() { "The password should be at least 8 characters long but should not exceed 50 characters." } });
            }


            if (valObj.Fields.Count > 0)
            {
                var refinedValObj = new ValidationObject()
                {
                    Fields = new List<ValidationObject.FieldValidation>()
                };
                foreach(ValidationObject.FieldValidation fv in valObj.Fields)
                {
                    var field = refinedValObj.Fields.SingleOrDefault(m => m.Field == fv.Field);
                    if (field != null)
                    {
                        field.ErrorMessages.Add(fv.ErrorMessages.First());
                    }
                    else
                    {
                        refinedValObj.Fields.Add(new ValidationObject.FieldValidation() { Field = fv.Field, ErrorMessages = fv.ErrorMessages });
                    }
                }

                return BadRequest(refinedValObj);
            }



            //Create user
            var user = new ApplicationUser()
            {
                Email = vm.Email,
                EmailVerified = false,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                RegistrationDatetime = DateTime.Now,
                PasswordHash = SecurePasswordHasher.Hash(vm.Password)
            };

            context.Users.Add(user);
            context.SaveChanges();



            //Send verification mail
            try
            {
                var emailVerification = new EmailVerification()
                {
                    UserID = user.ID,
                    Sent = DateTime.Now,
                    RandomString = Guid.NewGuid().ToString(),
                    Verified = false
                };

                SmtpClient client = new SmtpClient(config["Email:Server"])
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(config["Email:Username"], config["Email:Password"])
                };

                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(config["Email:Address"])
                };
                mailMessage.To.Add(user.Email);
                mailMessage.Body = "Dear " + user.LastName + " " + user.FirstName + ",<br>The following code lets you confirm your email '" + vm.Email + "' for your registration on the MyList website.<br><br>" + emailVerification.RandomString + "<br><br><br>Please, ignore and delete this email if you were not the one trying to register an account on MyList.<br>Thank you,<br>the MyList Team.";
                mailMessage.Subject = "MyList Email Verification";
                client.Send(mailMessage);

                context.EmailVerifications.Add(emailVerification);
                context.SaveChanges();
            }
            catch (Exception)
            {
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Email", ErrorMessages = new List<string>() { "We are experiencing problems with email verifications. Please, try again later." } } } });
            }

            return Ok(user.Email);
        }


        [HttpPost("[action]")]
        public ActionResult SendAgain([FromBody]string Email)
        {
            if (Email == null)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Email", ErrorMessages = new List<string>() { "This field is required." } } } });
            var attr = new EmailAddressAttribute();
            if (!attr.IsValid(Email))
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Email", ErrorMessages = new List<string>() { "Invalid email address." } } } });
            var user = context.Users.SingleOrDefault(m => m.Email == Email);
            if(user == null)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Email", ErrorMessages = new List<string>() { "The email address you entered is not registered." } } } });
            else
            {
                if (user.EmailVerified)
                    return Ok("We sent a new verification code to your email address.");
                else
                {
                    var emailVerification = context.EmailVerifications.SingleOrDefault(m => m.UserID == user.ID);
                    if(emailVerification == null)
                    {
                        emailVerification = new EmailVerification()
                        {
                            UserID = user.ID,
                            Sent = DateTime.Now,
                            Verified = false,
                            RandomString = Guid.NewGuid().ToString()
                        };
                    }
                    else
                    {
                        emailVerification.RandomString = Guid.NewGuid().ToString();
                        emailVerification.Sent = DateTime.Now;
                        context.SaveChanges();
                    }

                    SmtpClient client = new SmtpClient(config["Email:Server"])
                    {
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(config["Email:Username"], config["Email:Password"])
                    };

                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(config["Email:Address"])
                    };
                    mailMessage.To.Add(user.Email);
                    mailMessage.Body = "Dear " + user.LastName + " " + user.FirstName + ",<br>The following code lets you confirm your email '" + user.Email + "' for your registration on the MyList website.<br><br>" + emailVerification.RandomString + "<br><br><br>Please, ignore and delete this email if you were not the one trying to register an account on MyList.<br>Thank you,<br>the MyList Team.";
                    mailMessage.Subject = "MyList Email Verification";
                    client.Send(mailMessage);
                    return Ok("We sent a new verification code to your email address.");
                }
            }
        }


        [HttpPost("[action]")]
        public ActionResult Verify([FromBody]VerifyVM vm)
        {
            //Validation
            if (vm.Email == null)
                return BadRequest();
            if (vm.U == null)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "U", ErrorMessages = new List<string>() { "Please, enter the verification code we sent to your email address." } } } });


            var user = context.Users.SingleOrDefault(m => m.Email == vm.Email);
            if (user == null)
                return NotFound();
            if (user.EmailVerified)
                return Ok("You already verified your email address.");

            var emailVerification = context.EmailVerifications.SingleOrDefault(m => m.UserID == user.ID);
            if (emailVerification == null)
                return NotFound();

            if(emailVerification.RandomString == vm.U)
            {
                user.EmailVerified = true;
                context.SaveChanges();
                return Ok("Your email address was verified successfully.");
            }
            else
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "U", ErrorMessages = new List<string>() { "The code you entered doesn't match with the one we sent you. Please check your inbox and enter the correct code." } } } });
        }


        private string BuildToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim("ID",user.ID+""),
                new Claim("Email",user.Email),
            };

            var token = new JwtSecurityToken(config["Jwt:Issuer"],
              config["Jwt:Issuer"],
              claims: claims,
              expires: DateTime.Now.AddMinutes(30),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        class TokenAndEmail
        {
            public string Email { get; set; }
            public string Token { get; set; }
        }


        [HttpPost("[action]")]
        public ActionResult Login([FromBody]LoginVM vm)
        {
            //Validation
            var valObj = new ValidationObject()
            {
                Fields = new List<ValidationObject.FieldValidation>()
            };
            //Email and Password
            if(vm.Email == null)
                valObj.Fields.Add(new ValidationObject.FieldValidation() { Field = "Email", ErrorMessages = new List<string>() { "This field is required." } });
            if (vm.Password == null)
                valObj.Fields.Add(new ValidationObject.FieldValidation() { Field = "Password", ErrorMessages = new List<string>() { "This field is required." } });

            //Login
            var user = context.Users.SingleOrDefault(m => m.Email == vm.Email && SecurePasswordHasher.Verify(vm.Password, m.PasswordHash));
            if(user == null)
            {
                return Unauthorized();
            }
            else
            {
                //Generate token
                var token = BuildToken(user);
                //Return email and token
                return Ok(new TokenAndEmail() { Email = user.Email, Token = token });
            }
        }


        class UserInfo
        {
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime RegistrationDateTime { get; set; }
        }


        [HttpGet("[action]")]
        [Authorize]
        public ActionResult GetUserInfo()
        {
            //Auth
            var u = HttpContext.User;
            if (!int.TryParse(u.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();
            var user = context.Users.Find(ID);
            if (user == null)
                return Forbid();

            return Ok(new UserInfo()
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RegistrationDateTime = user.RegistrationDatetime
            });
        }


        [HttpPost("[action]")]
        [Authorize]
        public ActionResult ChangePassword([FromBody]ChangePasswordVM vm)
        {
            //Auth
            var u = HttpContext.User;
            if (!int.TryParse(u.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();
            var user = context.Users.Find(ID);
            if (user == null)
                return Forbid();

            //Validation
            var valObj = new ValidationObject()
            {
                Fields = new List<ValidationObject.FieldValidation>()
            };

            //OldPassword
            if(vm.OldPassword == null)
                valObj.Fields.Add(new ValidationObject.FieldValidation() { Field = "OldPassword", ErrorMessages = new List<string>() { "This field is required." } });
            if(!SecurePasswordHasher.Verify(vm.OldPassword,user.PasswordHash))
                valObj.Fields.Add(new ValidationObject.FieldValidation() { Field = "OldPassword", ErrorMessages = new List<string>() { "Wrong password." } });

            //NewPassword
            if(vm.NewPassword == null)
                valObj.Fields.Add(new ValidationObject.FieldValidation() { Field = "NewPassword", ErrorMessages = new List<string>() { "This field is required." } });
            else
            {
                if (!vm.NewPassword.Any(Char.IsLower) || !vm.NewPassword.Any(Char.IsUpper))
                    valObj.Fields.Add(new ValidationObject.FieldValidation() { Field = "NewPassword", ErrorMessages = new List<string>() { "The password should contain at least an uppercase character as well as a lowercase one." } });
                if (!vm.NewPassword.Any(Char.IsNumber))
                    valObj.Fields.Add(new ValidationObject.FieldValidation() { Field = "NewPassword", ErrorMessages = new List<string>() { "The password should contain at least a number." } });
                if (vm.NewPassword.Length < 8 || vm.NewPassword.Length > 50)
                    valObj.Fields.Add(new ValidationObject.FieldValidation() { Field = "NewPassword", ErrorMessages = new List<string>() { "The password should be at least 8 characters long but should not exceed 50 characters." } });
            }


            if (valObj.Fields.Count > 0)
            {
                var refinedValObj = new ValidationObject()
                {
                    Fields = new List<ValidationObject.FieldValidation>()
                };
                foreach (ValidationObject.FieldValidation fv in valObj.Fields)
                {
                    var field = refinedValObj.Fields.SingleOrDefault(m => m.Field == fv.Field);
                    if (field != null)
                    {
                        field.ErrorMessages.Add(fv.ErrorMessages.First());
                    }
                    else
                    {
                        refinedValObj.Fields.Add(new ValidationObject.FieldValidation() { Field = fv.Field, ErrorMessages = fv.ErrorMessages });
                    }
                }

                return BadRequest(refinedValObj);
            }

            user.PasswordHash = SecurePasswordHasher.Hash(vm.NewPassword);
            context.SaveChanges();

            //Notify user via mail

            return Ok("Your password was successfully changed.");
        }


        [HttpPost("[action]")]
        [Authorize]
        public ActionResult DeleteAccount()
        {
            //Auth
            var u = HttpContext.User;
            if (!int.TryParse(u.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();
            var user = context.Users.Find(ID);
            if (user == null)
                return Forbid();

            context.Users.Remove(user);
            context.SaveChanges();
            return Ok("Your account was successfully deleted.");
        }



        [HttpGet("[action]")]
        public ActionResult ResetPassword()
        {
            //Auth
            var u = HttpContext.User;
            if (!int.TryParse(u.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();
            var user = context.Users.Find(ID);
            if (user == null)
                return Forbid();

            //PasswordReset object
            var prevPR = context.PasswordResets.SingleOrDefault(m => m.UserID == user.ID);
            if(prevPR != null)
            {
                context.PasswordResets.Remove(prevPR);
                context.SaveChanges();
            }
            var passwordReset = new PasswordReset() { UserID = user.ID, Sent = DateTime.Now, RandomString = Guid.NewGuid().ToString() };
            try
            {
                SmtpClient client = new SmtpClient(config["Email:Server"])
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(config["Email:Username"], config["Email:Password"])
                };

                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(config["Email:Address"])
                };
                mailMessage.To.Add(user.Email);
                mailMessage.Body = "Dear " + user.LastName + " " + user.FirstName + ",<br>As you requested, here is the code to enter on the MyList website to reset your password.<br>The code will expire in 30 minutes.<br><br>" + passwordReset.RandomString + "<br><br><br>Please, ignore and delete this email if you were not the one trying to reset your password on MyList.<br>Thank you,<br>the MyList Team.";
                mailMessage.Subject = "MyList Password Reset";
                client.Send(mailMessage);

                context.PasswordResets.Add(passwordReset);
                context.SaveChanges();

                return Ok("The code for your password reset has been sent to your email address.");
            }
            catch (Exception)
            {
                return BadRequest("We are experiencing problems with our email server. Please, try again later.");
            }
        }


        [HttpPost("[action]")]
        [Authorize]
        public ActionResult ResetPassword([FromBody]string code)
        {
            //Auth
            var u = HttpContext.User;
            if (!int.TryParse(u.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();
            var user = context.Users.Find(ID);
            if (user == null)
                return Forbid();

            //Validation
            if(code == null)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Code", ErrorMessages = new List<string>() { "Please, enter the code we sent you via email." } } } });
            var passwordReset = context.PasswordResets.SingleOrDefault(m => m.UserID == user.ID);
            if (passwordReset == null)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Code", ErrorMessages = new List<string>() { "The code you entered doesn't match." } } } });
            if(passwordReset.Sent.AddMinutes(30) < DateTime.Now)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Code", ErrorMessages = new List<string>() { "The code you entered expired." } } } });
            if (passwordReset.RandomString == code)
            {
                //Send mail with temporary password
                try
                {
                    var newPassword = Guid.NewGuid().ToString();
                    user.PasswordHash = SecurePasswordHasher.Hash(newPassword);

                    SmtpClient client = new SmtpClient(config["Email:Server"])
                    {
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(config["Email:Username"], config["Email:Password"])
                    };

                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(config["Email:Address"])
                    };
                    mailMessage.To.Add(user.Email);
                    mailMessage.Body = "Dear " + user.LastName + " " + user.FirstName + ",<br>As you requested, here is the new password for your account '" + user.Email + "'.<br><br>" + newPassword + "<br><br><br>We advise you to change it as soon as possible.<br><br>Please, ignore and delete this email if you think this email was not meant for you.<br>Thank you,<br>the MyList Team.";
                    mailMessage.Subject = "MyList New Password";
                    client.Send(mailMessage);

                    context.SaveChanges();

                    return Ok("We sent a new password to your email address.");
                }
                catch (Exception)
                {
                    return BadRequest("We are experiencing problems with our email server. Please, try again later.");
                }
            }
            else
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Code", ErrorMessages = new List<string>() { "The code you entered doesn't match." } } } } );
        }
    }
}
