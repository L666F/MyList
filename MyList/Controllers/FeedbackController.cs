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
    [Route("api/Feedback")]
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext context;

        public FeedbackController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpPost("report")]
        [Authorize]
        public async Task<ActionResult> BugReport([FromBody]BugVM vm)
        {
            if (ModelState.IsValid)
            {
                var user = HttpContext.User;
                if (!Int32.TryParse(user.Claims.FirstOrDefault(c => c.Type == "ID").Value, out int ID))
                    return Forbid();

                var bug = new Bug()
                {
                    UserID = ID,
                    Title = vm.Title,
                    Text = vm.Text,
                    Date = DateTime.Now
                };

                await context.BugReports.AddAsync(bug);
                await context.SaveChangesAsync();
                return Ok(bug);
            }
            else
            {
                return BadRequest(ModelState.FirstOrDefault().Value.Errors.FirstOrDefault().ErrorMessage);
            }
        }
    }
}