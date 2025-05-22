using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace ViolationEditorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluationController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EvaluationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("summary")]
        public IActionResult GetSummary()
        {
            var dt = new DataTable();
            string connStr = _configuration.GetConnectionString("DefaultConnection");

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("SELECT * FROM vw_Evaluation_Summary", conn);
            using var adapter = new SqlDataAdapter(cmd);

            adapter.Fill(dt);
            return Ok(dt);
        }
    }
}
