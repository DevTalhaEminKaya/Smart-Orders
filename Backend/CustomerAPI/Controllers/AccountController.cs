using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Backend.Shared.Entities.DTOs.Account;
using Backend.Shared.Entities.Models;
using Backend.Shared.DataAccess;

namespace ClientApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase 
    {
        private readonly SmartOrdersDbContext _context;
        private int _userId;

        public AccountController(SmartOrdersDbContext context)
        {
            _context = context;
        }

        // ----------------------------------------------------------------------------------------------------

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<GetAccountDto>>> GetAccounts()
        {
            // Token'dan gelen userId'yi al
            _userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (!_context.Users.Any(x => x.Id == _userId))
            {
                return Forbid("You do not have permission to access.");
            }

            var accounts = await _context.Accounts
                .Where(x => x.UserId == _userId)
                .Select(account => new GetAccountDto
                {
                    Email = account.Email,
                    Password = account.Password
                }).ToListAsync();

            return Ok(accounts);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<GetAccountDto>> GetAccount(int id)
        {
            // Token'dan gelen userId'yi al
            _userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            if (!_context.Users.Any(x => x.Id == _userId))
            {
                return Forbid("You do not have permission to access.");
            }

            if (!_context.Accounts.Any(x => x.Id == id))
            {
                return NotFound("Account not found.");
            }

            var account = await _context.Accounts.FindAsync(id);

            if (_userId != account.UserId)
            {
                return Forbid("You do not have permission to access.");
            }

            var result = new GetAccountDto
            {
                WebsiteId = account.WebsiteId,
                Email = account.Email,
                Password = account.Password
            };

            return result;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateAccount(CreateAccountDto dto)
        {
            // Token'dan gelen userId'yi al
            _userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (!_context.Users.Any(x => x.Id == _userId))
            {
                return Forbid("You do not have permission to access.");
            }

            if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
            {
                return BadRequest("Email and Password cannot be empty.");
            }

            if (!await _context.Websites.AnyAsync(x => x.Id == dto.WebsiteId && x.IsActive))
            {
                return NotFound("Website not found.");
            }

            if (await _context.Accounts.AnyAsync(w => w.WebsiteId == dto.WebsiteId && w.UserId == _userId))
            {   
                return BadRequest("There can be only one account for the same website.");
            }

            var account = new Account
            {
                UserId = _userId,
                WebsiteId = dto.WebsiteId,
                Email = dto.Email,
                Password = dto.Password,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpPut("{id}/email")]
        public async Task<IActionResult> UpdateAccountEmail(int id, string email)
        {
            // Token'dan gelen userId'yi al
            _userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (!_context.Users.Any(x => x.Id == _userId))
            {
                return Forbid("You do not have permission to access.");
            }

            if (!_context.Accounts.Any(x => x.Id == id))
            {
                return NotFound("Account not found.");
            }

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email cannot be empty.");
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            if (_userId != account.UserId)
            {
                return Forbid("You do not have permission to access.");
            }

            account.Email = email;
            account.UpdatedAt = DateTime.UtcNow;
            account.UpdatedBy = "System";

            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpPut("{id}/password")]
        public async Task<IActionResult> UpdateAccountPassword(int id, string password)
        {
            // Token'dan gelen userId'yi al
            _userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (!_context.Users.Any(x => x.Id == _userId))
            {
                return Forbid("You do not have permission to access.");
            }

            if (!_context.Accounts.Any(x => x.Id == id))
            {
                return NotFound("Account not found.");
            }

            if (string.IsNullOrEmpty(password))
            {
                return BadRequest("Password cannot be empty.");
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            if (_userId != account.UserId)
            {
                return Forbid("You do not have permission to access.");
            }

            account.Password = password;
            account.UpdatedAt = DateTime.UtcNow;
            account.UpdatedBy = "System";

            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWebsite(int id)
        {
            _userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (!_context.Users.Any(x => x.Id == _userId))
            {
                return Forbid("You do not have permission to access.");
            }

            if (!_context.Accounts.Any(x => x.Id == id))
            {
                return NotFound("Account not found.");
            }

            var account = await _context.Accounts.FindAsync(id);

            if (_userId != account.UserId)
            {
                return Unauthorized();
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}