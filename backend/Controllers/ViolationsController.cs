using Microsoft.AspNetCore.Mvc;
using ViolationEditorApi.context;
using ViolationEditorApi.Models;

namespace ViolationEditorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ViolationsController : ControllerBase
    {
        private readonly ViolationDbContext _context;

        public ViolationsController(ViolationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TblCSQ>> Get()
        {
            var violations = _context.TblCSQ.ToList();
            return Ok(violations);
        }
    }
}
