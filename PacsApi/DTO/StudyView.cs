namespace PacsApi.DTO
{
    public class StudyView
    {
        public string PatientId { get; set; }
        public string PatientName { get; set; }
        public string PatientSex { get; set; }
        public string PatientAge { get; set; }

        public string StudyInstanceUid { get; set; }
        public string StudyId { get; set; }

        public DateTime? StudyDate { get; set; }
        public DateTime? StudyTime { get; set; }

        public string StudyDescription { get; set; }
        public string AccessionNumber { get; set; }

        public string ReferringPhysicianName { get; set; }
        public string PerformingPhysician { get; set; }
        public string InstitutionName { get; set; }

        public int SeriesCount { get; set; }
        public int ImageCount { get; set; }

 
        public string Modalities { get; set; } // e.g. "CT\\MR"
        public string BodyPartExamined { get; set; }
    }
}
