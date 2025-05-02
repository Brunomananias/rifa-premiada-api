using API_Rifa.Data;
using API_Rifa.Models;
using Microsoft.AspNetCore.Mvc;

namespace API_Rifa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
        public class PlansController : ControllerBase
        {
            private readonly AppDbContext _context;

            public PlansController(AppDbContext context)
            {
                _context = context;
            }

            // GET: api/plans
            [HttpGet]
            public ActionResult<IEnumerable<Plan>> GetPlans()
            {
                var plans = _context.Plans.ToList();
                return Ok(plans);
            }

        // GET: api/plans/{id}
        [HttpGet("{id}")]
        public ActionResult<Plan> GetPlanById(int id)
        {
            var plan = _context.Plans.FirstOrDefault(p => p.Id == id);

            if (plan == null)
            {
                return NotFound(new { message = "Plano não encontrado." });
            }

            return Ok(plan);
        }

    }
}

