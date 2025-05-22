using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using ViolationEditorApi.context;
using ViolationEditorApi.Models;

namespace ViolationEditorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TblCSQFileController : ControllerBase
    {
        private readonly ViolationDbContext _context;
        private readonly IConfiguration _configuration;

        public TblCSQFileController(ViolationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("UploadExcel")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("❌ الملف غير موجود أو فارغ.");

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var dt = new DataTable();
            dt.Columns.Add("Node_Session_Seq", typeof(string));
            dt.Columns.Add("CallStartTime", typeof(DateTime));
            dt.Columns.Add("CallEndTime", typeof(DateTime));
            dt.Columns.Add("ContactDisposition", typeof(int));
            dt.Columns.Add("OriginatorDN", typeof(string));
            dt.Columns.Add("DestinationDN", typeof(string));
            dt.Columns.Add("CalledNumber", typeof(string));
            dt.Columns.Add("ApplicationName", typeof(string));
            dt.Columns.Add("CSQNames", typeof(string));
            dt.Columns.Add("QueueTime", typeof(TimeSpan));
            dt.Columns.Add("AgentName", typeof(string));
            dt.Columns.Add("RingTime", typeof(TimeSpan));
            dt.Columns.Add("TalkTime", typeof(TimeSpan));
            dt.Columns.Add("WorkTime", typeof(TimeSpan));

            using var stream = file.OpenReadStream();
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var table = reader.AsDataSet().Tables[0];
            var rows = table.Rows.Cast<DataRow>().ToList();

            // تجاهل أول سطرين
            if (rows.Count > 2)
                rows = rows.Skip(2).ToList();

            // تجاهل آخر سطرين إذا احتووا على عبارات معينة
            if (rows.Count >= 2)
            {
                var lastRow1 = string.Join(" ", rows[^1].ItemArray).ToLower();
                var lastRow2 = string.Join(" ", rows[^2].ItemArray).ToLower();

                if (lastRow1.Contains("generated on") && lastRow1.Contains("records"))
                    rows.RemoveAt(rows.Count - 1);

                if (lastRow2.Contains("filter interval") && lastRow2.Contains("application name"))
                    rows.RemoveAt(rows.Count - 1);
            }

            // التاريخ المطلوب فلترته
            DateTime targetDate = new DateTime(2025, 5, 7);

            foreach (var row in rows)
            {
                if (row == null || row.ItemArray.All(f => string.IsNullOrWhiteSpace(f?.ToString())))
                    continue;

                DateTime? ParseDate(string? v) => DateTime.TryParse(v, out var d) ? d : null;
                TimeSpan? ParseTime(string? v) => TimeSpan.TryParse(v, out var t) ? t : null;
                int? ParseInt(string? v) => int.TryParse(v, out var r) ? r : null;
                string? Trunc(string? v, int len) => string.IsNullOrWhiteSpace(v) ? null : v.Length > len ? v.Substring(0, len) : v;

                var callStart = ParseDate(row[1]?.ToString());
                if (callStart == null || callStart.Value.Date != targetDate.Date)
                    continue;

                var nodeSession = Trunc(row[0]?.ToString(), 100);
                if (string.IsNullOrWhiteSpace(nodeSession))
                    continue;

                var dataRow = dt.NewRow();
                dataRow["Node_Session_Seq"] = nodeSession;
                dataRow["CallStartTime"] = callStart.Value;
                dataRow["CallEndTime"] = ParseDate(row[2]?.ToString()) ?? (object)DBNull.Value;
                dataRow["ContactDisposition"] = ParseInt(row[3]?.ToString()) ?? (object)DBNull.Value;
                dataRow["OriginatorDN"] = Trunc(row[4]?.ToString(), 50);
                dataRow["DestinationDN"] = Trunc(row[5]?.ToString(), 50);
                dataRow["CalledNumber"] = Trunc(row[6]?.ToString(), 20);
                dataRow["ApplicationName"] = Trunc(row[7]?.ToString(), 100);
                dataRow["CSQNames"] = Trunc(row[8]?.ToString(), 200);
                dataRow["QueueTime"] = ParseTime(row[9]?.ToString()) ?? (object)DBNull.Value;
                dataRow["AgentName"] = Trunc(row[10]?.ToString(), 100);
                dataRow["RingTime"] = ParseTime(row[11]?.ToString()) ?? (object)DBNull.Value;
                dataRow["TalkTime"] = ParseTime(row[12]?.ToString()) ?? (object)DBNull.Value;
                dataRow["WorkTime"] = ParseTime(row[13]?.ToString()) ?? (object)DBNull.Value;

                dt.Rows.Add(dataRow);
            }

            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await conn.OpenAsync();

                using var bulkCopy = new SqlBulkCopy(conn)
                {
                    DestinationTableName = "tbl_CSQ"
                };

                foreach (DataColumn column in dt.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                await bulkCopy.WriteToServerAsync(dt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ خطأ أثناء الحفظ: {ex.Message}");
            }

            return Ok($"✅ تم رفع الملف بنجاح. عدد السجلات: {dt.Rows.Count}");
        }
    }
}
