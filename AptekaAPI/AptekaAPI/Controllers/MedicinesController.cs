using AptekaAPI.Data;
using AptekaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AptekaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicinesController : ControllerBase
    {
        private readonly AptekaContext _context;
        public MedicinesController(AptekaContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetMedicines([FromQuery] string? search)
        {
            var query = _context.KursMedicines
                .Include(m => m.Category)
                .Include(m => m.Supplier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.Name.Contains(search) || m.Manufacturer.Contains(search));

            var result = await query.Select(m => new
            {
                m.IdMedicine,
                m.Name,
                m.Manufacturer,
                m.Price,
                m.Amount,
                CategoryName = m.Category.Name
            }).ToListAsync();

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> CreateMedicine([FromBody] KursMedicine medicine)
        {
            if (medicine.Price < 0 || medicine.Amount < 0)
                return BadRequest(new { error = "Цена и количество не могут быть отрицательными" });

            _context.KursMedicines.Add(medicine);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Лекарство добавлено", id = medicine.IdMedicine });
        }
    }
}