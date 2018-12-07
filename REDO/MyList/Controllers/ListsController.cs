using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyList.Models;
using MyList.Models.Data;
using MyList.Models.VMs;

namespace MyList.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListsController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ListsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [Authorize]
        [HttpGet("[action]")]
        public ActionResult GetLists()
        {
            //Auth
            var u = HttpContext.User;
            if (!int.TryParse(u.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();
            var user = context.Users.Find(ID);
            if (user == null)
                return Forbid();

            //Read from DB
            var lists = context.Lists.Where(m => m.UserID == user.ID);
            foreach(ListClass list in lists)
            {
                list.ListItems = context.ListItems.Where(m => m.ListID == list.ID).ToList();
            }

            //Return lists
            return Ok(lists);
        }

        [Authorize]
        [HttpGet("[action]")]
        public ActionResult GetSharedLists()
        {
            //Auth
            var u = HttpContext.User;
            if (!int.TryParse(u.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();
            var user = context.Users.Find(ID);
            if (user == null)
                return Forbid();

            //Search for shared lists for current user
            var userlists = context.UserLists.Where(m => m.UserID == user.ID);
            var lists = new List<ListClass>();

            foreach(UserList ul in userlists)
            {
                var list = context.Lists.SingleOrDefault(m => m.ID == ul.ListID);
                if (list == null)
                {
                    context.UserLists.Remove(ul);
                    context.SaveChanges();
                }
                else
                    lists.Add(list);
            }

            return Ok(lists);
        }

        [Authorize]
        [HttpGet("[action]")]
        public ActionResult GetAllLists()
        {
            //Auth
            var u = HttpContext.User;
            if (!int.TryParse(u.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();
            var user = context.Users.Find(ID);
            if (user == null)
                return Forbid();

            //Search for shared lists for current user
            var userlists = context.UserLists.Where(m => m.UserID == user.ID);
            var lists = new List<ListClass>();

            foreach (UserList ul in userlists)
            {
                var list = context.Lists.SingleOrDefault(m => m.ID == ul.ListID);
                if (list == null)
                {
                    context.UserLists.Remove(ul);
                    context.SaveChanges();
                }
                else
                    lists.Add(list);
            }

            //Search for user's lists
            lists.AddRange(context.Lists.Where(m => m.UserID == user.ID));

            return Ok(lists);
        }

        [Authorize]
        [HttpPost("[action]")]
        public ActionResult NewList([FromBody]NewListVM vm)
        {
            //Auth
            var u = HttpContext.User;
            if (!int.TryParse(u.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();
            var user = context.Users.Find(ID);
            if (user == null)
                return Forbid();

            //Validation
            if(vm.Title == null)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Title", ErrorMessages = new List<string>() { "This field is required." } } } });
            if(vm.Title.Length<1 || vm.Title.Length>50)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Title", ErrorMessages = new List<string>() { "The title should be between 1 and 50 characters long." } } } });

            var newList = new ListClass()
            {
               Title = vm.Title,
               UserID = user.ID
            };

            context.Lists.Add(newList);
            context.SaveChanges();

            return Ok(newList.ID);
        }

        [Authorize]
        [HttpPost("[action]")]
        public ActionResult DeleteList([FromBody]int? id)
        {
            //Auth
            var u = HttpContext.User;
            if (!int.TryParse(u.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();
            var user = context.Users.Find(ID);
            if (user == null)
                return Forbid();

            //Validation
            if (id == null)
                return BadRequest();
            var list = context.Lists.SingleOrDefault(m => m.ID == id);
            if (list == null)
                return NotFound();
            if (list.UserID != user.ID)
                return Forbid();

            //Delete list
            context.ListItems.RemoveRange(context.ListItems.Where(m => m.ListID == list.ID));
            context.UserLists.RemoveRange(context.UserLists.Where(m => m.ListID == list.ID));
            context.Lists.Remove(list);
            context.SaveChanges();

            return Ok();
        }

        [Authorize]
        [HttpPost("[action]")]
        public ActionResult EditTitle([FromBody]string title,int? id)
        {
            //Auth
            var u = HttpContext.User;
            if (!int.TryParse(u.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();
            var user = context.Users.Find(ID);
            if (user == null)
                return Forbid();

            //Validation
            if (id == null)
                return BadRequest();
            var list = context.Lists.SingleOrDefault(m => m.ID == id);
            if (list == null)
                return NotFound();
            if (list.UserID != user.ID)
                return Forbid();
            if(title == null)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Title", ErrorMessages = new List<string>() { "This field is required." } } } });
            if(title.Length<1 || title.Length>50)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Title", ErrorMessages = new List<string>() { "The title should be between 1 and 50 characters long." } } } });

            //Edit title
            list.Title = title;
            context.SaveChanges();

            return Ok();
        }

        [Authorize]
        [HttpPost("[action]")]
        public ActionResult AddListItem([FromBody]AddListItemVM vm)
        {
            //Auth
            var u = HttpContext.User;
            if (!int.TryParse(u.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();
            var user = context.Users.Find(ID);
            if (user == null)
                return Forbid();

            //Validation
            //Title
            if(vm.Title == null)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Title", ErrorMessages = new List<string>() { "This field is required." } } } });
            if(vm.Title.Length<1 || vm.Title.Length>50)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Title", ErrorMessages = new List<string>() { "This field should be between 1 and 50 characters long." } } } });
            //Note
            if(vm.Note != null)
                if(vm.Note.Length<1 || vm.Note.Length>200)
                    return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Note", ErrorMessages = new List<string>() { "The note should be between 1 and 200 characters long." } } } });
            //Checked
            if (vm.Checked == null)
                vm.Checked = false;
            //ListID
            if (vm.ListID == null)
                return BadRequest();
            var list = context.Lists.SingleOrDefault(m => m.ID == vm.ListID);
            if (list == null)
                return NotFound();
            if (list.UserID != user.ID)
                return Forbid();

            //Add ListItem
            var listItem = new ListItem()
            {
                ListID = list.ID,
                Title = vm.Title,
                Note = vm.Note,
                Checked = (bool)vm.Checked
            };

            context.ListItems.Add(listItem);
            context.SaveChanges();

            return Ok();
        }

        [Authorize]
        [HttpPost("[action]")]
        public ActionResult DeleteListItem([FromBody]int? id)
        {
            //Auth
            var u = HttpContext.User;
            if (!int.TryParse(u.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();
            var user = context.Users.Find(ID);
            if (user == null)
                return Forbid();

            //Validation
            if (id == null)
                return BadRequest();
            var listItem = context.ListItems.SingleOrDefault(m => m.ID == id);
            if (listItem == null)
                return NotFound();
            var list = context.Lists.SingleOrDefault(m => m.ID == listItem.ListID);
            if (list == null)
                return NotFound();
            if (list.UserID != user.ID)
                return Forbid();

            //Remove ListItem
            context.ListItems.Remove(listItem);
            context.SaveChanges();

            return Ok();
        }

        [Authorize]
        [HttpPost("[action]")]
        public ActionResult EditListItem([FromBody]EditListItemVM vm)
        {
            //Auth
            var u = HttpContext.User;
            if (!int.TryParse(u.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();
            var user = context.Users.Find(ID);
            if (user == null)
                return Forbid();

            //Validation
            //Title
            if (vm.Title == null)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Title", ErrorMessages = new List<string>() { "This field is required." } } } });
            if (vm.Title.Length < 1 || vm.Title.Length > 50)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Title", ErrorMessages = new List<string>() { "This field should be between 1 and 50 characters long." } } } });
            //Note
            if (vm.Note != null)
                if (vm.Note.Length < 1 || vm.Note.Length > 200)
                    return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Note", ErrorMessages = new List<string>() { "The note should be between 1 and 200 characters long." } } } });
            //Checked
            var ckd = false;
            if (vm.Checked != null)
                ckd = (bool)vm.Checked;
            //ID
            if (vm.ID == null)
                return BadRequest();
            var listItem = context.ListItems.SingleOrDefault(m => m.ID == vm.ID);
            if (listItem == null)
                return NotFound();
            var list = context.Lists.SingleOrDefault(m => m.ID == listItem.ListID);
            if (list == null)
                return NotFound();
            if (list.UserID != user.ID)
                return Forbid();

            //Edit ListItem
            listItem.Title = vm.Title;
            listItem.Note = vm.Note;
            listItem.Checked = ckd;

            context.SaveChanges();

            return Ok();
        }


        [Authorize]
        [HttpPost("[action]")]
        public ActionResult ShareWithEmail([FromBody]ShareWithEmailVM vm)
        {
            //Auth
            var u = HttpContext.User;
            if (!int.TryParse(u.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();
            var user = context.Users.Find(ID);
            if (user == null)
                return Forbid();

            //Validation
            //ListID
            if (vm.ListID == null)
                return BadRequest();
            var list = context.Lists.SingleOrDefault(m => m.ID == vm.ListID);
            if (list == null)
                return NotFound();
            if (list.UserID != user.ID)
                return Forbid();
            //Email
            if(vm.Email == null)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Email", ErrorMessages = new List<string>() { "This field is required." } } } });
            var userToShareWith = context.Users.SingleOrDefault(m => m.Email == vm.Email);
            if (userToShareWith == null)
                return NotFound();
            if(userToShareWith == user)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Email", ErrorMessages = new List<string>() { "Please, enter another user's email." } } } });
            if (context.UserLists.SingleOrDefault(m => m.ListID == list.ID && m.UserID == userToShareWith.ID) != null)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Email", ErrorMessages = new List<string>() { "You already shared this list with this user." } } } });

            //Share
            var userlist = new UserList()
            {
                ListID = list.ID,
                UserID = userToShareWith.ID,
                SharedDateTime = DateTime.Now
            };

            context.UserLists.Add(userlist);
            context.SaveChanges();

            return Ok();
        }


        [Authorize]
        [HttpPost("[action]")]
        public ActionResult UnshareWithEmail([FromBody]ShareWithEmailVM vm)
        {
            //Auth
            var u = HttpContext.User;
            if (!int.TryParse(u.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();
            var user = context.Users.Find(ID);
            if (user == null)
                return Forbid();

            //Validation
            //ListID
            if (vm.ListID == null)
                return BadRequest();
            var list = context.Lists.SingleOrDefault(m => m.ID == vm.ListID);
            if (list == null)
                return NotFound();
            if (list.UserID != user.ID)
                return Forbid();
            //Email
            if (vm.Email == null)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Email", ErrorMessages = new List<string>() { "This field is required." } } } });
            var userToShareWith = context.Users.SingleOrDefault(m => m.Email == vm.Email);
            if (userToShareWith == null)
                return NotFound();
            if (userToShareWith == user)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Email", ErrorMessages = new List<string>() { "Please, enter another user's email." } } } });
            var userlist = context.UserLists.SingleOrDefault(m => m.ListID == list.ID && m.UserID == userToShareWith.ID);
            if (userlist == null)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Email", ErrorMessages = new List<string>() { "You are not sharing this list with this user." } } } });

            //Unshare
            context.UserLists.Remove(userlist);
            context.SaveChanges();

            return Ok();
        }


        [Authorize]
        [HttpPost("[action]")]
        public ActionResult GiveOwnership([FromBody]ShareWithEmailVM vm)
        {
            //Auth
            var u = HttpContext.User;
            if (!int.TryParse(u.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();
            var user = context.Users.Find(ID);
            if (user == null)
                return Forbid();

            //Validation
            //ListID
            if (vm.ListID == null)
                return BadRequest();
            var list = context.Lists.SingleOrDefault(m => m.ID == vm.ListID);
            if (list == null)
                return NotFound();
            if (list.UserID != user.ID)
                return Forbid();
            //Email
            if (vm.Email == null)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Email", ErrorMessages = new List<string>() { "This field is required." } } } });
            var userToShareWith = context.Users.SingleOrDefault(m => m.Email == vm.Email);
            if (userToShareWith == null)
                return NotFound();
            if (userToShareWith == user)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Email", ErrorMessages = new List<string>() { "Please, enter another user's email." } } } });
            var userlist = context.UserLists.SingleOrDefault(m => m.ListID == list.ID && m.UserID == userToShareWith.ID);
            if (userlist == null)
                return BadRequest(new ValidationObject() { Fields = new List<ValidationObject.FieldValidation>() { new ValidationObject.FieldValidation() { Field = "Email", ErrorMessages = new List<string>() { "Before giving this user the ownership of this list, you have to be sharing the list with this user." } } } });

            //Give ownership
            list.UserID = userToShareWith.ID;
            var OwnerUserList = new UserList()
            {
                ListID = list.ID,
                UserID = user.ID,
                SharedDateTime = userlist.SharedDateTime
            };
            context.UserLists.Remove(userlist);
            context.UserLists.Add(OwnerUserList);
            context.SaveChanges();

            return Ok();
        }
    }
}
