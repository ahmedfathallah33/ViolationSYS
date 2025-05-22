using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViolationEditorApi.Models
{
    [Table("tbl_CSQ")] // ✅ هذا هو السطر المهم
    public class TblCSQ
    {
        public int ID { get; set; }
        public string? Node_Session_Seq { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? CallStartTime { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? CallEndTime { get; set; }

        public int? ContactDisposition { get; set; }
        public string? OriginatorDN { get; set; }
        public string? DestinationDN { get; set; }
        public string? CalledNumber { get; set; }
        public string? ApplicationName { get; set; }
        public string? CSQNames { get; set; }
        public TimeSpan? QueueTime { get; set; }
        public string? AgentName { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public TimeSpan? RingTime { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public TimeSpan? TalkTime { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public TimeSpan? WorkTime { get; set; }

        public string? CallSurvey { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? Violation_ABD { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? Violation_Ring { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? Violation_CallSurvey { get; set; }
    }
}
