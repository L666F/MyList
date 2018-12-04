using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyList.Models;
using MyList.Models.Data;
using MyList.Models.ViewModels;

namespace MyList.Controllers
{
    [Produces("application/json")]
    [Route("api/List")]
    public class ListController : Controller
    {
        private readonly ApplicationDbContext context;

        private class ProductVMLocal
        {
            public int ID { get; set; }
            public int ProductID { get; set; }
            public string Name { get; set; }
            public int Category { get; set; }
            public string Note { get; set; }
        }

        private class ListAndProducts
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public List<ProductVMLocal> Products { get; set; }
        }

        public ListController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public async Task<ActionResult> GetList(int id)
        {
            var user = HttpContext.User;
            if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();

            var list = await context.Lists.FindAsync(id);
            if (list == null)
                return NotFound();

            if (context.UserLists.SingleOrDefault(m => m.UserID == ID && m.ListID == id) == null && ID != list.UserID)
                return Forbid();

            var listproducts = context.ListProducts.Where(m => m.ListID == list.ID);
            var allProducts = context.Products.ToList();
            var products = new List<ProductVMLocal>();

            foreach(ListProduct el in listproducts)
            {
                var p = allProducts.SingleOrDefault(m => m.ID == el.ProductID);
                if (p != null)
                {
                    products.Add(new ProductVMLocal() { ID = el.ID, Name = p.Name, Category = p.Category, Note = el.Note, ProductID = p.ID });
                }
            }
            

            return Ok(new ListAndProducts() { ID = list.ID, Name = list.Name, Products = products });
        }

        [HttpGet("products")]
        [Authorize]
        public ActionResult GetProducts()
        {
            var user = HttpContext.User;
            if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();

            return Ok(context.Products.ToList());
        }

        [HttpGet("getall")]
        [Authorize]
        public ActionResult GetAllLists()
        {
            var user = HttpContext.User;
            if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();

            var lists = new List<ListAndProducts>();
            var currentUsersLists = context.Lists.Where(m => m.UserID == ID).ToList();
            foreach(UserList ul in context.UserLists)
            {
                if (ul.UserID == ID && !currentUsersLists.Contains(context.Lists.Find(ul.ListID)))
                    currentUsersLists.Add(context.Lists.Find(ul.ListID));
            }
            foreach(MyList.Models.List l in currentUsersLists)
            {
                var listproducts = context.ListProducts.Where(m => m.ListID == l.ID);
                var allProducts = context.Products.ToList();
                var products = new List<ProductVMLocal>();

                foreach (ListProduct el in listproducts)
                {
                    var p = allProducts.SingleOrDefault(m => m.ID == el.ProductID);
                    if (p != null)
                    {
                        products.Add(new ProductVMLocal() { ID = el.ID, Name = p.Name, Category = p.Category, Note = el.Note, ProductID = p.ID });
                    }
                }

                var listToAdd = new ListAndProducts() { ID = l.ID, Name = l.Name, Products = products };
                lists.Add(listToAdd);
            }
            return Ok(lists);

        }

        [HttpPost("new")]
        [Authorize]
        public async Task<ActionResult> NewList([FromBody]ListVM vm)
        {
            var user = HttpContext.User;
            if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();

            if (ModelState.IsValid)
            {
                if (context.Lists.SingleOrDefault(m => m.UserID == ID && m.Name == vm.Name) != null)
                    return BadRequest("You are already using this list name.");

                var list = new List()
                {
                    Name = vm.Name,
                    UserID = ID
                };

                context.Lists.Add(list);
                await context.SaveChangesAsync();
                return Ok(vm);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [Authorize]
        [HttpDelete("delete/{id}")]
        public ActionResult DeleteList(int id)
        {
            var user = HttpContext.User;
            if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();

            var listInDb = context.Lists.Find(id);
            if (listInDb == null)
                return NotFound();

            if (listInDb.UserID != ID)
                return Forbid();

            context.ListProducts.RemoveRange(context.ListProducts.Where(m => m.ListID == id));
            context.Lists.Remove(listInDb);
            context.SaveChangesAsync();

            return Ok();
        }

        /*[Authorize]
        [HttpPost("rename/{id}")]
        public async Task<ActionResult> RenameList(int id, [FromBody]ListVM vm)
        {
            var user = HttpContext.User;
            if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();

            var listInDb = await context.Lists.FindAsync(id);
            if (listInDb == null)
                return NotFound();

            if (listInDb.UserID != ID)
                return Forbid();

            if (ModelState.IsValid)
            {
                listInDb.Name = vm.Name;
                await context.SaveChangesAsync();
                return Ok(vm);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }*/

        [HttpPost("empty/{id}")]
        [Authorize]
        public async Task<ActionResult> EmptyList(int id)
        {
            var user = HttpContext.User;
            if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();

            var listInDb = await context.Lists.FindAsync(id);
            if (listInDb == null)
                return NotFound();

            if (context.UserLists.SingleOrDefault(m => m.UserID == ID && m.ListID == id) == null && ID != listInDb.UserID)
                return Forbid();

            context.ListProducts.RemoveRange(context.ListProducts.Where(m => m.ListID == listInDb.ID));
            await context.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpPost("add/{id}")]
        public async Task<ActionResult> AddProducts(int id, [FromBody]ProductVM vm)
        {
            var user = HttpContext.User;
            if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();

            var listInDb = await context.Lists.FindAsync(id);
            if (listInDb == null)
                return NotFound();

            if (context.UserLists.SingleOrDefault(m => m.UserID == ID && m.ListID == id) == null && ID != listInDb.UserID)
                return Forbid();

            if (ModelState.IsValid)
            {
                foreach(ProductVM.MyProduct p in vm.Products)
                {
                    if (context.Products.Find(p.ID) != null)
                        context.ListProducts.Add(new ListProduct() { ListID = id, Note = p.Note, ProductID = p.ID });
                }
                await context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [Authorize]
        [HttpPost("remove/{id}")]
        public async Task<ActionResult> RemoveProducts(int id, [FromBody]RemoveProductVM vm)
        {
            var user = HttpContext.User;
            if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();

            var listInDb = await context.Lists.FindAsync(id);
            if (listInDb == null)
                return NotFound();

            if (context.UserLists.SingleOrDefault(m => m.UserID == ID && m.ListID == id) == null && ID != listInDb.UserID)
                return Forbid();

            if (ModelState.IsValid)
            {
                foreach(int listProductId in vm.IDs)
                {
                    var el = context.ListProducts.Find(listProductId);
                    if (el!=null)
                        context.ListProducts.Remove(el);
                }
                await context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        public class InviteVMLocal
        {
            [Required]
            public List<string> Users { get; set; }
        }

        [Authorize]
        [HttpPost("invite/{id}")]
        public async Task<ActionResult> InviteUsersToList(int id,[FromBody]InviteVMLocal vm)
        {
            var user = HttpContext.User;
            if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();

            var listInDb = await context.Lists.FindAsync(id);
            if (listInDb == null)
                return NotFound();

            if (context.UserLists.SingleOrDefault(m => m.UserID == ID && m.ListID == id) == null && ID != listInDb.UserID)
                return Forbid();

            if (ModelState.IsValid)
            {
                foreach(string username in vm.Users)
                {
                    var invited = context.Users.SingleOrDefault(m => m.Username == username);
                    if (invited != null)
                    {
                        if (context.Invites.SingleOrDefault(m => m.InvitedID == invited.ID && m.InviterID == ID) == null)
                        {
                            var invite = new InviteUserList() { InvitedID = invited.ID, InviterID = ID, Date = DateTime.Now, ListID = id };
                            await context.Invites.AddAsync(invite);
                        }
                    }
                }
                await context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        public class ResponseVM
        {
            [Required]
            public int InviteID { get; set; }
            [Required]
            public bool Accepted { get; set; }
        }

        [Authorize]
        [HttpPost("response")]
        public async Task<ActionResult> ResponseToInvite([FromBody]ResponseVM vm)
        {
            var user = HttpContext.User;
            if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();

            if (ModelState.IsValid)
            {
                var invite = await context.Invites.FindAsync(vm.InviteID);
                if (invite == null)
                    return NotFound();

                if (invite.InvitedID != ID)
                    return Forbid();

                if (vm.Accepted)
                {
                    var userlist = new UserList() { ListID = invite.ListID, UserID = ID };
                    await context.UserLists.AddAsync(userlist);
                }
                else
                {
                    context.Invites.Remove(invite);
                }

                await context.SaveChangesAsync();
                
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        private class InvitesVMLocal
        {
            public List<InviteUserList> InvitesIN { get; set; }
            public List<InviteUserList> InvitesOUT { get; set; }
        }

        [Authorize]
        [HttpGet("invites")]
        public ActionResult GetInvites()
        {
            var user = HttpContext.User;
            if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();

            var IN = context.Invites.Where(m => m.InvitedID == ID).ToList();
            var OUT = context.Invites.Where(m => m.InviterID == ID).ToList();
            return Ok(new InvitesVMLocal() { InvitesIN = IN, InvitesOUT = OUT });

        }

        [Authorize]
        [HttpDelete("invites/{id}")]
        public ActionResult DeleteInvite(int id)
        {
            var user = HttpContext.User;
            if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                return Forbid();

            var invite = context.Invites.Find(id);
            if (invite == null)
                return NotFound();

            if (invite.InviterID != ID)
                return Forbid();

            context.Invites.Remove(invite);
            context.SaveChangesAsync();
            return Ok();
        }
    }
}