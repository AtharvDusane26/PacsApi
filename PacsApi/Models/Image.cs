using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PacsApi.Models
{
    public class Image
    {
        [Key]
        public string SopInstanceUid { get; set; }

        public string SeriesInstanceUid { get; set; }
        public string StudyInstanceUid { get; set; }
        public string PatientId { get; set; }

        public virtual Series Series { get; set; }
        public string FilePath { get; set; }
        public string SopClassUid { get; set; }
        public string TransferSyntaxUid { get; set; }
        public int? InstanceNumber { get; set; }

        public int? Rows { get; set; }
        public int? Columns { get; set; }

        public int? BitsAllocated { get; set; }
        public int? BitsStored { get; set; }
        public int? HighBit { get; set; }
        public int? PixelRepresentation { get; set; }
        public string PhotometricInterpretation { get; set; }
        public int? SamplesPerPixel { get; set; }
        public string ImagePositionPatient { get; set; }    
        public string ImageOrientationPatient { get; set; }  
        public string PixelSpacing { get; set; }           
        public string SliceThickness { get; set; }        
        public string FrameOfReferenceUid { get; set; }
        public string? Modality { get; set; }
        public string? BodyPartExamined { get; set; }
        public string? ProtocolName { get; set; }
        public DateTime? AcquisitionTime { get; set; }
        public double? RescaleSlope { get; set; }
        public double? RescaleIntercept { get; set; }
        public string Kvp { get; set; }
        public string XrayTubeCurrent { get; set; }

        public string EchoTime { get; set; }
        public string RepetitionTime { get; set; }
        public string FlipAngle { get; set; }
        public int? FrameCount { get; set; }
        public string? FrameTime { get; set; }
        public string? CineRate { get; set; }
        public string ImageType { get; set; }
        public string ConvolutionKernel { get; set; }

        public string? AccessionNumber { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not Image other)
                return false;

            return string.Equals(SopInstanceUid, other.SopInstanceUid, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return SopInstanceUid?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return $"Image: {SopInstanceUid}";
        }
    }
}
