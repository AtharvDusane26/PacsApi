using System.ComponentModel.DataAnnotations;

namespace PacsApi.Models
{
    public class Series
    {
        [Key]
        public string SeriesInstanceUid { get; set; }
        public string StudyInstanceUid { get; set; }
        public string PatientId { get; set; }
        public virtual Study Study { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public string Modality { get; set; }
        public int? SeriesNumber { get; set; }
        public DateTime? SeriesDate { get; set; }
        public DateTime? SeriesTime { get; set; }
        public string SeriesDescription { get; set; }
        public string BodyPartExamined { get; set; }
        public string ProtocolName { get; set; }
        public string PatientPosition { get; set; }
        public string SendingAETitle { get; set; }
        public int? NumberOfSeriesRelatedInstances { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not Series other)
                return false;

            return string.Equals(SeriesInstanceUid, other.SeriesInstanceUid, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return SeriesInstanceUid?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return $"Series: {SeriesInstanceUid}, Modality: {Modality}";
        }
    }
}
