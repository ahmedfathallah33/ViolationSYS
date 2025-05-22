
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.IO;
using ExcelDataReader;

namespace ViolationEditorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CallSurveyFileController : ControllerBase
    {
        private readonly string _filePath = @"C:\LV\ReportsEditor\Report_Files\CallSurvey.xlsx";

        [HttpGet]
        public IActionResult Get()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            if (!System.IO.File.Exists(_filePath))
            {
                return NotFound("الملف غير موجود.");
            }

            using var stream = System.IO.File.Open(_filePath, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var result = reader.AsDataSet();

            var table = result.Tables[0]; // أول شيت
            var data = new List<Dictionary<string, object>>();

            for (int i = 1; i < table.Rows.Count; i++) // نبدأ من السطر الثاني (عشان العناوين)
            {
                var row = new Dictionary<string, object>();
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    var header = table.Rows[0][j]?.ToString();
                    var value = table.Rows[i][j];
                    row[header ?? $"Column{j}"] = value;
                }
                data.Add(row);
            }

            return Ok(data);
        }
    }
}
