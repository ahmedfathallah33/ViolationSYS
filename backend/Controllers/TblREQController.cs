using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using ViolationEditorApi.context;
using ViolationEditorApi.Models;

namespace ViolationEditorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadREQController : ControllerBase
    {
        private readonly ViolationDbContext _context;
        private readonly IConfiguration _configuration;

        public UploadREQController(ViolationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("UploadExcel")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("الملف غير موجود أو فارغ.");

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            DataTable dt = new DataTable();
            dt.Columns.Add("Submit_Date_Time", typeof(DateTime));
            dt.Columns.Add("Approved_Date", typeof(DateTime));
            dt.Columns.Add("Responded_Date", typeof(DateTime));
            dt.Columns.Add("Completion_Date_Time", typeof(DateTime));
            dt.Columns.Add("Closed_Date_Time", typeof(DateTime));
            dt.Columns.Add("Re_Opened_Date", typeof(DateTime));
            dt.Columns.Add("Last_Resolved_Date", typeof(DateTime));

            using var stream = file.OpenReadStream();
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var result = reader.AsDataSet();
            var table = result.Tables[0];

            for (int i = 1; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                if (row == null || row.ItemArray.All(f => string.IsNullOrWhiteSpace(f?.ToString())))
                    continue;

                DateTime? ParseDate(string? v) => DateTime.TryParse(v, out var d) ? d : null;

                var dataRow = dt.NewRow();
                dataRow["Submit_Date_Time"] = ParseDate(row[0]?.ToString()) ?? (object)DBNull.Value;
                dataRow["Approved_Date"] = ParseDate(row[1]?.ToString()) ?? (object)DBNull.Value;
                dataRow["Responded_Date"] = ParseDate(row[2]?.ToString()) ?? (object)DBNull.Value;
                dataRow["Completion_Date_Time"] = ParseDate(row[3]?.ToString()) ?? (object)DBNull.Value;
                dataRow["Closed_Date_Time"] = ParseDate(row[4]?.ToString()) ?? (object)DBNull.Value;
                dataRow["Re_Opened_Date"] = ParseDate(row[5]?.ToString()) ?? (object)DBNull.Value;
                dataRow["Last_Resolved_Date"] = ParseDate(row[6]?.ToString()) ?? (object)DBNull.Value;

                dt.Rows.Add(dataRow);
            }

            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await conn.OpenAsync();

                using var bulkCopy = new SqlBulkCopy(conn)
                {
                    DestinationTableName = "tbl_REQ"
                };

                bulkCopy.ColumnMappings.Add("Submit_Date_Time", "Submit_Date_Time");
                bulkCopy.ColumnMappings.Add("Approved_Date", "Approved_Date");
                bulkCopy.ColumnMappings.Add("Responded_Date", "Responded_Date");
                bulkCopy.ColumnMappings.Add("Completion_Date_Time", "Completion_Date_Time");
                bulkCopy.ColumnMappings.Add("Closed_Date_Time", "Closed_Date_Time");
                bulkCopy.ColumnMappings.Add("Re_Opened_Date", "Re_Opened_Date");
                bulkCopy.ColumnMappings.Add("Last_Resolved_Date", "Last_Resolved_Date");

                await bulkCopy.WriteToServerAsync(dt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ خطأ أثناء الحفظ باستخدام SqlBulkCopy: {ex.Message}");
            }

            return Ok($"✅ تم رفع الملف وتخزين البيانات بنجاح. عدد السجلات: {dt.Rows.Count}");
        }

        [HttpGet("TestREQModel")]
        public async Task<IActionResult> TestREQModel()
        {
            try
            {
                var record = await _context.TblREQ.FirstOrDefaultAsync();

                if (record == null)
                    return Ok("✅ الاتصال سليم، لكن لا توجد بيانات في جدول tbl_REQ.");

                return Ok(record);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ خطأ: {ex.Message}");
            }
        }
    }
}
