namespace PacsApi.DTO
{
    public class ImageView
    {
        public string SopInstanceUid { get; set; }
        public string SeriesInstanceUid { get; set; }
        public int? InstanceNumber { get; set; }
        public string FilePath { get; set; }
    }
}
