using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace ViolationEditorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TblCallSurveyController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public TblCallSurveyController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("UploadExcel")]
        public IActionResult UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("❌ الملف غير موجود أو فارغ.");

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            DataTable dt = new DataTable();
            dt.Columns.Add("Node_Session_Seq", typeof(string));
            dt.Columns.Add("SurveyResponse", typeof(string));
            dt.Columns.Add("CreatedDate", typeof(DateTime));

            using var stream = file.OpenReadStream();
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var result = reader.AsDataSet();
            var table = result.Tables[0];

            for (int i = 1; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                if (row == null || row.ItemArray.All(f => string.IsNullOrWhiteSpace(f?.ToString())))
                    continue;

                DateTime? ParseDateTime(string? v) => DateTime.TryParse(v, out var r) ? r : null;

                var dataRow = dt.NewRow();
                dataRow["Node_Session_Seq"] = row[1]?.ToString();
                dataRow["SurveyResponse"] = row[2]?.ToString();
                dataRow["CreatedDate"] = ParseDateTime(row[3]?.ToString()) ?? (object)DBNull.Value;

                dt.Rows.Add(dataRow);
            }

            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                conn.Open();

                using var bulkCopy = new SqlBulkCopy(conn)
                {
                    DestinationTableName = "dbo.TblCallSurvey" // ✅ التعديل هنا
                };

                bulkCopy.ColumnMappings.Add("Node_Session_Seq", "Node_Session_Seq");
                bulkCopy.ColumnMappings.Add("SurveyResponse", "SurveyResponse");
                bulkCopy.ColumnMappings.Add("CreatedDate", "CreatedDate");

                bulkCopy.WriteToServer(dt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ خطأ أثناء الحفظ باستخدام SqlBulkCopy: {ex.Message}");
            }

            return Ok($"✅ تم رفع الملف وتخزين البيانات بنجاح. عدد السجلات: {dt.Rows.Count}");
        }
    }
}
