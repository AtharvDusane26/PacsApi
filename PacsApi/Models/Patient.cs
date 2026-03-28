using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PacsApi.Models
{
    public class Patient 
    {
        [Key]
        public string PatientId { get; set; }

        public string PatientName { get; set; }
        public string PatientSex { get; set; }
        public int? Age { get; set; }
        public string AgeString { get; set; }

        public DateTime? PatientBirthDate { get; set; }

        public int? ExaminingDoctorId { get; set; }

        public virtual ICollection<Study> Studies { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not Patient other)
                return false;

            return string.Equals(PatientId, other.PatientId, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return PatientId?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return $"PatientId: {PatientId}, PatientName: {PatientName}";
        }
    }
}
