using PacsApi.Models;

namespace PacsApi.DataManagement
{
    public interface IDbHandler
    {
        // ==================== PATIENT ====================

        Task AddPatient(Patient patient);
        Task<Patient> GetPatientById(string id);
        Task<List<Patient>> GetAllPatients();
        void UpdatePatient(Patient patient);
        Task DeletePatientById(string id);

        // ==================== STUDY ====================

        Task AddStudy(Study study);
        Task<Study> GetStudyById(string id);
        Task<List<Study>> GetStudiesByPatientId(string patientId);
        Task<List<Study>> GetAllStudies();
        void UpdateStudy(Study study);
        Task DeleteStudyById(string id);

        // ==================== SERIES ====================

        Task AddSeries(Series series);
        Task<Series> GetSeriesById(string id);
        Task<List<Series>> GetSeriesByStudyInstanceUid(string studyInstanceUid);
        void UpdateSeries(Series series);
        Task DeleteSeriesById(string id);

        // ==================== IMAGE ====================

        Task AddImage(Image image);
        Task<Image> GetImageBySopInstanceUid(string sopInstanceUid);
        Task<List<Image>> GetImagesBySeriesInstanceUid(string seriesInstanceUid);
        void UpdateImage(Image image);
        Task DeleteImageById(string sopInstanceUid);

        Task<bool> ImageExist(string sopInstanceUid);
        // ==================== SAVE ====================

        Task SaveAsync(); // 🔥 VERY IMPORTANT
    }

}
