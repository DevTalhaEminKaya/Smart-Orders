using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Backend.Shared.Entities.Enums;
using Backend.Shared.Entities.Models;
using Backend.Shared.DataAccess;
using Backend.Shared.Entities.DTOs.Account;
using Backend.Shared.Entities.DTOs.Webise;

namespace AdminApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebsiteController : ControllerBase 
    {
        private readonly SmartOrdersDbContext _context;
        
        public int _userId;

        public WebsiteController(SmartOrdersDbContext context)
        {
            _context = context;
        }

        // ----------------------------------------------------------------------------------------------------

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<GetWebsiteDto>>> GetWebsites()
        {
            return await _context.Websites
                .Where(x => x.IsActive)
                .Select(x => new GetWebsiteDto
                {
                    Name = x.Name,
                    HomeUrl = x.HomeUrl
                })
                .ToListAsync();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<GetWebsiteDto>> GetWebsiteById(int id)
        {
            return await _context.Websites
                .Where(x => x.Id == id && x.IsActive)
                .Select(x => new GetWebsiteDto
                {
                    Name = x.Name,
                    HomeUrl = x.HomeUrl
                })
                .FirstAsync();
        }
    }
}