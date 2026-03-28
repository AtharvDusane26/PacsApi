using Microsoft.EntityFrameworkCore;
using PacsApi.Context;
using PacsApi.Models;

namespace PacsApi.DataManagement
{
    using Microsoft.EntityFrameworkCore;
    using PacsApi.DTO;

    public class DBHandler
    {
        private readonly PacsDbContext _context;

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
            _context.Patients.Attach(patient);
            _context.Entry(patient).State = EntityState.Modified;
        }

        public async Task DeletePatientById(string id)
        {
            var patient = new Patient { PatientId = id };
            _context.Patients.Attach(patient);
            _context.Patients.Remove(patient);
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
        public async Task<List<StudyView>> GetAllStudyView()
        {
            // Step 1: Load base studies + patient
            var studies = await _context.Studies
                .AsNoTracking()
                .Include(s => s.Patient)
                .ToListAsync();

            var studyUids = studies.Select(s => s.StudyInstanceUid).ToList();

            // Step 2: Load series separately
            var seriesList = await _context.Series
                .AsNoTracking()
                .Where(s => studyUids.Contains(s.StudyInstanceUid))
                .ToListAsync();

            var seriesUids = seriesList.Select(s => s.SeriesInstanceUid).ToList();

            // Step 3: Load images separately
            var images = await _context.Images
                .AsNoTracking()
                .Where(i => seriesUids.Contains(i.SeriesInstanceUid))
                .ToListAsync();

            // Step 4: Build result in memory
            return studies.Select(s =>
            {
                var studySeries = seriesList
                    .Where(x => x.StudyInstanceUid == s.StudyInstanceUid)
                    .ToList();

                var studyImages = images
                    .Where(x => x.StudyInstanceUid == s.StudyInstanceUid)
                    .ToList();

                return new StudyView
                {
                    PatientId = s.PatientId,
                    PatientName = s.Patient.PatientName,
                    PatientSex = s.Patient.PatientSex,
                    PatientAge = s.Patient.AgeString,

                    StudyInstanceUid = s.StudyInstanceUid,
                    StudyDate = s.StudyDate,
                    StudyDescription = s.StudyDescription,

                    SeriesCount = studySeries.Count,
                    ImageCount = studyImages.Count,

                    Modalities = string.Join(",", studySeries.Select(x => x.Modality).Distinct()),
                    BodyPartExamined = studySeries.FirstOrDefault()?.BodyPartExamined
                };
            }).ToList();
        }
        public void UpdateStudy(Study study)
        {
            _context.Studies.Attach(study);
            _context.Entry(study).State = EntityState.Modified;
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
            _context.Series.Attach(series);
            _context.Entry(series).State = EntityState.Modified;
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
                .OrderBy(x => x.InstanceNumber) // ✅ important for correct order
                .ToListAsync();
        }
        public async Task<List<Image>> GetImagesByStudyInstanceUid(string studyInstanceUid)
        {
            return await _context.Images
                .AsNoTracking()
                .Where(x => x.StudyInstanceUid == studyInstanceUid)
                .OrderBy(x => x.InstanceNumber) // optional but good
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
            _context.Images.Attach(image);
            _context.Entry(image).State = EntityState.Modified;
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
    }
}
