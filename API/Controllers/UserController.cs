using System.Collections.Generic;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
   [Authorize]
    public class UserController : BaseApiController
    {
        private readonly DataContext _context;

        // Constructor to inject DataContext
        public UserController(DataContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: api/User
        [HttpGet]
        public async  Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();  // Fetch users from database
            return users;  // Return the list of users
        }

        [HttpGet("{id:int}")]
        public async Task <ActionResult<AppUser>> GetUser(int id){

            var user = await _context.Users.FindAsync(id);
            if (user == null ) return NotFound();
            return user;
        }
    }
}
