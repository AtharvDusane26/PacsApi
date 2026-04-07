using Microsoft.EntityFrameworkCore;
using PacsApi.Context;
using PacsApi.Models;
using PacsApi.DTO;
using EFCore.lib.Utility;
namespace PacsApi.DataManagement
{
    public class DBHandler : Handler<PacsDbContext>
    {
        public DBHandler(IUnitOfWork unitOfWork, PacsDbContext context) : base(unitOfWork, context) { }


        #region ==================== PATIENT ====================

        public async Task AddPatient(Patient patient)
        {
            await UnitOfWork.Repository<Patient>().AddAsync(patient);
        }

        public async Task<Patient?> GetPatientById(string id)
        {
            return await UnitOfWork
                .Repository<Patient>()
                .SingleOrDefaultAsync(x => x.PatientId == id);
        }

        public async Task<List<Patient>> GetAllPatients()
        {
            return await UnitOfWork
                .Repository<Patient>()
                .GetAllAsync();
        }

        public void UpdatePatient(Patient patient)
        {
            UnitOfWork.Repository<Patient>().UpdateAsync(patient);
        }

        public async Task DeletePatientById(string id)
        {
            var patient = new Patient { PatientId = id };
            await UnitOfWork.Repository<Patient>().DeleteAsync(patient);
        }

        public async Task DeleteAllPatients()
        {
            await Context.Patients.ExecuteDeleteAsync(); // keep optimized bulk
        }

        #endregion

        #region ==================== STUDY ====================

        public async Task AddStudy(Study study)
        {
            await UnitOfWork.Repository<Study>().AddAsync(study);
        }

        public async Task<Study?> GetStudyById(string id)
        {
            return await UnitOfWork
                .Repository<Study>()
                .SingleOrDefaultAsync(x => x.StudyInstanceUid == id);
        }

        public async Task<List<Study>> GetStudiesByPatientId(string patientId)
        {
            return await UnitOfWork
                .Repository<Study>()
                .GetFilteredAsync(x => x.PatientId == patientId);
        }

        public async Task<List<Study>> GetAllStudies()
        {
            return await UnitOfWork
                .Repository<Study>()
                .GetAllAsync();
        }

        // 🔥 KEEP COMPLEX QUERY DIRECT (Best Practice)
        public async Task<List<StudyView>> GetAllStudyView()
        {
            return await Context.Studies
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

                    SeriesCount = Context.Series.Count(se => se.StudyInstanceUid == s.StudyInstanceUid),
                    ImageCount = Context.Images.Count(i => i.StudyInstanceUid == s.StudyInstanceUid),

                    Modalities = string.Join(",",
                        Context.Series
                            .Where(se => se.StudyInstanceUid == s.StudyInstanceUid)
                            .Select(se => se.Modality)
                            .Distinct()),

                    BodyPartExamined = Context.Series
                        .Where(se => se.StudyInstanceUid == s.StudyInstanceUid)
                        .Select(se => se.BodyPartExamined)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        public void UpdateStudy(Study study)
        {
            UnitOfWork.Repository<Study>().UpdateAsync(study);
        }

        public async Task DeleteStudyById(string id)
        {
            var study = new Study { StudyInstanceUid = id };
            await UnitOfWork.Repository<Study>().DeleteAsync(study);
        }

        public async Task<List<string>> GetStudyInstanceUidsByPatientId(string patientId)
        {
            return await Context.Studies
                .AsNoTracking()
                .Where(x => x.PatientId == patientId)
                .Select(x => x.StudyInstanceUid)
                .ToListAsync();
        }

        public async Task<List<string>> GetAllStudyInstanceUids()
        {
            return await Context.Studies
                .AsNoTracking()
                .Select(x => x.StudyInstanceUid)
                .ToListAsync();
        }

        public async Task DeleteAllStudies()
        {
            await Context.Studies.ExecuteDeleteAsync();
        }

        #endregion

        #region ==================== SERIES ====================

        public async Task AddSeries(Series series)
        {
            await UnitOfWork.Repository<Series>().AddAsync(series);
        }

        public async Task<Series?> GetSeriesById(string id)
        {
            return await UnitOfWork
                .Repository<Series>()
                .SingleOrDefaultAsync(x => x.SeriesInstanceUid == id);
        }

        public async Task<List<Series>> GetSeriesByStudyInstanceUid(string studyInstanceUid)
        {
            return await UnitOfWork
                .Repository<Series>()
                .GetFilteredAsync(x => x.StudyInstanceUid == studyInstanceUid);
        }

        public void UpdateSeries(Series series)
        {
            UnitOfWork.Repository<Series>().UpdateAsync(series);
        }

        public async Task DeleteSeriesById(string id)
        {
            var series = new Series { SeriesInstanceUid = id };
            await UnitOfWork.Repository<Series>().DeleteAsync(series);
        }

        public async Task<List<string>> GetAllSeriesInstanceUids()
        {
            return await Context.Series
                .AsNoTracking()
                .Select(x => x.SeriesInstanceUid)
                .ToListAsync();
        }

        public async Task<List<string>> GetSeriesInstanceUidsByStudyInstanceUid(string studyInstanceUid)
        {
            return await Context.Series
                .AsNoTracking()
                .Where(x => x.StudyInstanceUid == studyInstanceUid)
                .Select(x => x.SeriesInstanceUid)
                .ToListAsync();
        }

        public async Task DeleteAllSeries()
        {
            await Context.Series.ExecuteDeleteAsync();
        }

        #endregion

        #region ==================== IMAGE ====================

        public async Task AddImage(Image image)
        {
            await UnitOfWork.Repository<Image>().AddAsync(image);
        }

        public async Task<Image?> GetImageBySopInstanceUid(string sopInstanceUid)
        {
            return await UnitOfWork
                .Repository<Image>()
                .SingleOrDefaultAsync(x => x.SopInstanceUid == sopInstanceUid);
        }

        public async Task<bool> ImageExist(string sopInstanceUid)
        {
            return await UnitOfWork
                .Repository<Image>()
                .AnyAsync(x => x.SopInstanceUid == sopInstanceUid);
        }

        public async Task<List<Image>> GetImagesBySeriesInstanceUid(string seriesInstanceUid)
        {
            return await Context.Images
                .AsNoTracking()
                .Where(x => x.SeriesInstanceUid == seriesInstanceUid)
                .OrderBy(x => x.InstanceNumber)
                .ToListAsync();
        }

        public async Task<List<Image>> GetImagesByStudyInstanceUid(string studyInstanceUid)
        {
            return await Context.Images
                .AsNoTracking()
                .Where(x => x.StudyInstanceUid == studyInstanceUid)
                .OrderBy(x => x.InstanceNumber)
                .ToListAsync();
        }

        public async Task<List<Image>> GetAllImages()
        {
            return await UnitOfWork
                .Repository<Image>()
                .GetAllAsync();
        }

        public async Task<List<ImageView>> GetImageViews(string studyInstanceUid)
        {
            return await Context.Images
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
            UnitOfWork.Repository<Image>().UpdateAsync(image);
        }

        public async Task DeleteImageById(string sopInstanceUid)
        {
            var image = new Image { SopInstanceUid = sopInstanceUid };
            await UnitOfWork.Repository<Image>().DeleteAsync(image);
        }

        public async Task<List<string>> GetAllSopInstanceUids()
        {
            return await Context.Images
                .AsNoTracking()
                .Select(x => x.SopInstanceUid)
                .ToListAsync();
        }

        public async Task DeleteAllImages()
        {
            await Context.Images.ExecuteDeleteAsync();
        }
        public async Task<string?> GetImagePath(string sopInstanceUid)
        {
            return await Context.Images
                .AsNoTracking()
                .Where(x => x.SopInstanceUid == sopInstanceUid)
                .Select(x => x.FilePath)
                .FirstOrDefaultAsync();
        }
        public async Task<List<string>> GetAllImagePaths()
        {
            return await Context.Images
                .AsNoTracking()
                .Select(x => x.FilePath)
                .ToListAsync();
        }

        #endregion

    }
}
