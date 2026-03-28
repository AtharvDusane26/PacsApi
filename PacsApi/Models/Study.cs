using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PacsApi.Models
{
    public class Study 
    {
        [Key]
        public string StudyInstanceUid { get; set; }
        public string PatientId { get; set; }
        public virtual Patient Patient { get; set; }

        public virtual ICollection<Series> Series { get; set; }
        public DateTime? StudyDate { get; set; }
        public DateTime? StudyTime { get; set; }

        public string StudyId { get; set; }
        public string AccessionNumber { get; set; }

        public string StudyDescription { get; set; }
        public string ReferringPhysicianName { get; set; }
        public string PerformingPhysician { get; set; }
        public string InstitutionName { get; set; }

        public int? ExaminingDoctorId { get; set; }
        public int? NumberOfStudyRelatedInstances { get; set; }
        public int? NumberOfStudyRelatedSeries { get; set; }
        public bool? IsDicomSent { get; set; }
        public bool? IsAutoRouted { get; set; }
        public bool? IsPrinted { get; set; }
        public bool? IsCdWritten { get; set; }

        public bool? IsArchived { get; set; }
        public string? Report { get; set; }
        public string? ReportStatus { get; set; }
        public string? SendingAETitle { get; set; }
        public DateTime? ReceivingDate { get; set; }
        public double? PatientWeight { get; set; }
        public double? PatientSize { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not Study other)
                return false;

            return string.Equals(StudyInstanceUid, other.StudyInstanceUid, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return StudyInstanceUid?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return $"Study: {StudyInstanceUid}, Desc: {StudyDescription}";
        }
    }
}
