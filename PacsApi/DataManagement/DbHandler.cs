using Microsoft.EntityFrameworkCore;
using PacsApi.Context;
using PacsApi.Models;

namespace PacsApi.DataManagement
{
    using Microsoft.EntityFrameworkCore;
    using PacsApi.DTO;

    public class DBHandler : IDisposable
    {
        private PacsDbContext _context;
        private bool _disposedValue;

        public DBHandler(PacsDbContext context)
        {
            _context = context;
        }

        // ==================== PATIENT ====================

        public async Task AddPatient(Patient patient)
        {
            await _context.Patients.AddAsync(patient);
        }

        public async Task<Patient?> GetPatientById(string id)
        {
            return await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PatientId == id);
        }

        public async Task<List<Patient>> GetAllPatients()
        {
            return await _context.Patients
                .AsNoTracking()
                .ToListAsync();
        }

        public void UpdatePatient(Patient patient)
        {
            _context.Patients.Update(patient);
        }

        public Task DeletePatientById(string id)
        {
            var patient = new Patient { PatientId = id };
            _context.Patients.Attach(patient);
            _context.Patients.Remove(patient);
            return Task.CompletedTask;
        }

        public async Task DeleteAllPatients()
        {
            await _context.Patients.ExecuteDeleteAsync();
        }

        // ==================== STUDY ====================

        public async Task AddStudy(Study study)
        {
            await _context.Studies.AddAsync(study);
        }

        public async Task<Study?> GetStudyById(string id)
        {
            return await _context.Studies
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.StudyInstanceUid == id);
        }

        public async Task<List<Study>> GetStudiesByPatientId(string patientId)
        {
            return await _context.Studies
                .AsNoTracking()
                .Where(x => x.PatientId == patientId)
                .ToListAsync();
        }

        public async Task<List<Study>> GetAllStudies()
        {
            return await _context.Studies
                .AsNoTracking()
                .ToListAsync();
        }

        // 🔥 FULLY OPTIMIZED (Single SQL Query)
        public async Task<List<StudyView>> GetAllStudyView()
        {
            return await _context.Studies
                .AsNoTracking()
                .Select(s => new StudyView
                {
                    PatientId = s.PatientId,
                    PatientName = s.Patient.PatientName,
                    PatientSex = s.Patient.PatientSex,
                    PatientAge = s.Patient.AgeString,

                    StudyInstanceUid = s.StudyInstanceUid,
                    StudyDate = s.StudyDate,
                    StudyDescription = s.StudyDescription,

                    SeriesCount = _context.Series
                        .Count(se => se.StudyInstanceUid == s.StudyInstanceUid),

                    ImageCount = _context.Images
                        .Count(i => i.StudyInstanceUid == s.StudyInstanceUid),

                    Modalities = string.Join(",",
                        _context.Series
                            .Where(se => se.StudyInstanceUid == s.StudyInstanceUid)
                            .Select(se => se.Modality)
                            .Distinct()),

                    BodyPartExamined = _context.Series
                        .Where(se => se.StudyInstanceUid == s.StudyInstanceUid)
                        .Select(se => se.BodyPartExamined)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        public void UpdateStudy(Study study)
        {
            _context.Studies.Update(study);
        }

        public Task DeleteStudyById(string id)
        {
            var study = new Study { StudyInstanceUid = id };
            _context.Studies.Attach(study);
            _context.Studies.Remove(study);
            return Task.CompletedTask;
        }

        public async Task<List<string>> GetStudyInstanceUidsByPatientId(string patientId)
        {
            return await _context.Studies
                .AsNoTracking()
                .Where(x => x.PatientId == patientId)
                .Select(x => x.StudyInstanceUid)
                .ToListAsync();
        }

        public async Task<List<string>> GetAllStudyInstanceUids()
        {
            return await _context.Studies
                .AsNoTracking()
                .Select(x => x.StudyInstanceUid)
                .ToListAsync();
        }

        public async Task DeleteAllStudies()
        {
            await _context.Studies.ExecuteDeleteAsync();
        }

        // ==================== SERIES ====================

        public async Task AddSeries(Series series)
        {
            await _context.Series.AddAsync(series);
        }

        public async Task<Series?> GetSeriesById(string id)
        {
            return await _context.Series
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SeriesInstanceUid == id);
        }

        public async Task<List<Series>> GetSeriesByStudyInstanceUid(string studyInstanceUid)
        {
            return await _context.Series
                .AsNoTracking()
                .Where(x => x.StudyInstanceUid == studyInstanceUid)
                .ToListAsync();
        }

        public void UpdateSeries(Series series)
        {
            _context.Series.Update(series);
        }

        public Task DeleteSeriesById(string id)
        {
            var series = new Series { SeriesInstanceUid = id };
            _context.Series.Attach(series);
            _context.Series.Remove(series);
            return Task.CompletedTask;
        }

        public async Task<List<string>> GetAllSeriesInstanceUids()
        {
            return await _context.Series
                .AsNoTracking()
                .Select(x => x.SeriesInstanceUid)
                .ToListAsync();
        }

        public async Task<List<string>> GetSeriesInstanceUidsByStudyInstanceUid(string studyInstanceUid)
        {
            return await _context.Series
                .AsNoTracking()
                .Where(x => x.StudyInstanceUid == studyInstanceUid)
                .Select(x => x.SeriesInstanceUid)
                .ToListAsync();
        }

        public async Task DeleteAllSeries()
        {
            await _context.Series.ExecuteDeleteAsync();
        }

        // ==================== IMAGE ====================

        public async Task AddImage(Image image)
        {
            await _context.Images.AddAsync(image);
        }

        public async Task<Image?> GetImageBySopInstanceUid(string sopInstanceUid)
        {
            return await _context.Images
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SopInstanceUid == sopInstanceUid);
        }

        public async Task<bool> ImageExist(string sopInstanceUid)
        {
            return await _context.Images
                .AsNoTracking()
                .AnyAsync(x => x.SopInstanceUid == sopInstanceUid);
        }

        public async Task<List<Image>> GetImagesBySeriesInstanceUid(string seriesInstanceUid)
        {
            return await _context.Images
                .AsNoTracking()
                .Where(x => x.SeriesInstanceUid == seriesInstanceUid)
                .OrderBy(x => x.InstanceNumber)
                .ToListAsync();
        }

        public async Task<List<Image>> GetImagesByStudyInstanceUid(string studyInstanceUid)
        {
            return await _context.Images
                .AsNoTracking()
                .Where(x => x.StudyInstanceUid == studyInstanceUid)
                .OrderBy(x => x.InstanceNumber)
                .ToListAsync();
        }

        public async Task<List<Image>> GetAllImages()
        {
            return await _context.Images
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<ImageView>> GetImageViews(string studyInstanceUid)
        {
            return await _context.Images
                .AsNoTracking()
                .Where(x => x.StudyInstanceUid == studyInstanceUid)
                .OrderBy(x => x.InstanceNumber)
                .Select(x => new ImageView
                {
                    SopInstanceUid = x.SopInstanceUid,
                    SeriesInstanceUid = x.SeriesInstanceUid,
                    InstanceNumber = x.InstanceNumber,
                    FilePath = x.FilePath
                })
                .ToListAsync();
        }

        public void UpdateImage(Image image)
        {
            _context.Images.Update(image);
        }

        public Task DeleteImageById(string sopInstanceUid)
        {
            var image = new Image { SopInstanceUid = sopInstanceUid };
            _context.Images.Attach(image);
            _context.Images.Remove(image);
            return Task.CompletedTask;
        }

        public async Task<List<string>> GetAllSopInstanceUids()
        {
            return await _context.Images
                .AsNoTracking()
                .Select(x => x.SopInstanceUid)
                .ToListAsync();
        }

        public async Task<List<string>> GetSopInstanceUidsBySeriesInstanceUid(string seriesInstanceUid)
        {
            return await _context.Images
                .AsNoTracking()
                .Where(x => x.SeriesInstanceUid == seriesInstanceUid)
                .Select(x => x.SopInstanceUid)
                .ToListAsync();
        }

        public async Task<List<string>> GetSopInstanceUidsByStudyInstanceUid(string studyInstanceUid)
        {
            return await _context.Images
                .AsNoTracking()
                .Where(x => x.StudyInstanceUid == studyInstanceUid)
                .Select(x => x.SopInstanceUid)
                .ToListAsync();
        }

        public async Task<List<string>> GetSopInstanceUidsByPatientId(string patientId)
        {
            return await _context.Images
                .AsNoTracking()
                .Where(x => x.PatientId == patientId)
                .Select(x => x.SopInstanceUid)
                .ToListAsync();
        }

        public async Task<List<string>> GetAllImagePaths()
        {
            return await _context.Images
                .AsNoTracking()
                .Select(x => x.FilePath)
                .ToListAsync();
        }

        public async Task<string?> GetImagePath(string sopInstanceUid)
        {
            return await _context.Images
                .AsNoTracking()
                .Where(x => x.SopInstanceUid == sopInstanceUid)
                .Select(x => x.FilePath)
                .FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetImagePathsBySeriesInstanceUid(string seriesInstanceUid)
        {
            return await _context.Images
                .AsNoTracking()
                .Where(x => x.SeriesInstanceUid == seriesInstanceUid)
                .Select(x => x.FilePath)
                .ToListAsync();
        }

        public async Task DeleteAllImages()
        {
            await _context.Images.ExecuteDeleteAsync();
        }

        // ==================== SAVE ====================

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _context.ChangeTracker.Clear();
                    _context.Dispose();
                    _context = null;
                }

                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
