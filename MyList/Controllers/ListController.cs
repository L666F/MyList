using System;
using System.Collections.Generic;
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

        private class ListAndProducts
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public List<Product> Products { get; set; }
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
            var products = new List<Product>();

            foreach(ListProduct el in listproducts)
            {
                var p = allProducts.SingleOrDefault(m => m.ID == el.ProductID);
                if (p != null)
                    products.Add(p);
            }
            

            return Ok(new ListAndProducts() { ID = list.ID, Name = list.Name, Products = products });
        }

        [HttpPost("new")]
        [Authorize]
        public ActionResult NewList([FromBody]ListVM vm)
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
                context.SaveChangesAsync();
                return Ok(vm);
            }
            else
            {
                return BadRequest(ModelState.FirstOrDefault().Value.Errors.FirstOrDefault().ErrorMessage);
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

        [Authorize]
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
                return BadRequest(ModelState.FirstOrDefault().Value.Errors.FirstOrDefault().ErrorMessage);
            }
        }

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
                return BadRequest(ModelState.FirstOrDefault().Value.Errors.FirstOrDefault().ErrorMessage);
            }
        }

        [Authorize]
        [HttpDelete("remove/{id}")]
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
                    context.ListProducts.Remove(context.ListProducts.Find(listProductId));
                }
                await context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return BadRequest(ModelState.FirstOrDefault().Value.Errors.FirstOrDefault().ErrorMessage);
            }
        }
    }
}