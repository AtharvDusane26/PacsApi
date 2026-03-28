using FellowOakDicom;
using PacsApi.DataManagement;
using PacsApi.Models;
using PacsApi.Authentication;
using PacsApi.DTO;

namespace PacsApi.Services
{
    public class DicomService
    {
        private readonly UserManager _userManager;
        public DicomService(UserManager userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> ProcessRawDicomStreamAsync(Stream dicomStream, string userId)
        {
            string filePath = string.Empty;
            var context = _userManager.GetUserDbContext(userId);
            var dbHandler = new DBHandler(context);

            try
            {
                var dicomFile = await DicomFile.OpenAsync(dicomStream, FileReadOption.Default);
                var ds = dicomFile.Dataset;

                // =========================
                // 🔥 Extract once (avoid repeated calls)
                // =========================
                string patientId = ds.GetSingleValueOrDefault(DicomTag.PatientID, "");
                string studyUid = ds.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, "");
                string seriesUid = ds.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, "");
                string sopUid = ds.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, "");

                // =========================
                // 🔥 Build file path
                // =========================
                string root = Path.Combine(
                    GeneralSettings.BaseDirectory,
                    "Images",
                    patientId,
                    studyUid,
                    seriesUid);

                Directory.CreateDirectory(root);

                filePath = Path.Combine(root, $"{sopUid}.dic");

                // =========================
                // 🔥 Fast exit (file exists)
                // =========================
                if (File.Exists(filePath))
                    return sopUid;

                // =========================
                // 🔥 DB existence check (lightweight)
                // =========================
                if (await dbHandler.ImageExist(sopUid))
                    return sopUid;

                // =========================
                // 🔥 PATIENT (UPSERT style)
                // =========================
                var patient = await dbHandler.GetPatientById(patientId);
                if (patient == null)
                {
                    patient = new Patient
                    {
                        PatientId = patientId,
                        PatientName = ds.GetSingleValueOrDefault(DicomTag.PatientName, ""),
                        PatientSex = ds.GetSingleValueOrDefault(DicomTag.PatientSex, ""),
                        AgeString = ds.GetSingleValueOrDefault(DicomTag.PatientAge, ""),
                        PatientBirthDate = ds.GetSingleValueOrDefault(DicomTag.PatientBirthDate, DateTime.MinValue)
                    };

                    try
                    {
                        await dbHandler.AddPatient(patient);
                    }
                    catch
                    {
                        // Another thread inserted → safe to ignore
                    }
                }

                // =========================
                // 🔥 STUDY
                // =========================
                var study = await dbHandler.GetStudyById(studyUid);
                if (study == null)
                {
                    study = new Study
                    {
                        StudyInstanceUid = studyUid,
                        PatientId = patientId,

                        StudyDate = ds.GetSingleValueOrDefault(DicomTag.StudyDate, DateTime.MinValue),
                        StudyTime = ds.GetSingleValueOrDefault(DicomTag.StudyTime, DateTime.MinValue),

                        StudyId = ds.GetSingleValueOrDefault(DicomTag.StudyID, ""),
                        AccessionNumber = ds.GetSingleValueOrDefault(DicomTag.AccessionNumber, ""),
                        StudyDescription = ds.GetSingleValueOrDefault(DicomTag.StudyDescription, ""),
                        ReferringPhysicianName = ds.GetSingleValueOrDefault(DicomTag.ReferringPhysicianName, ""),
                        PerformingPhysician = ds.GetSingleValueOrDefault(DicomTag.PerformingPhysicianName, ""),
                        InstitutionName = ds.GetSingleValueOrDefault(DicomTag.InstitutionName, ""),
                        PatientWeight = ds.GetSingleValueOrDefault(DicomTag.PatientWeight, 0.0),
                        PatientSize = ds.GetSingleValueOrDefault(DicomTag.PatientSize, 0.0),
                        NumberOfStudyRelatedInstances = ds.GetSingleValueOrDefault(DicomTag.NumberOfStudyRelatedInstances, 0),
                        NumberOfStudyRelatedSeries = ds.GetSingleValueOrDefault(DicomTag.NumberOfStudyRelatedSeries, 0),
                    };

                    try
                    {
                        await dbHandler.AddStudy(study);
                    }
                    catch
                    {
                        // ignore duplicate
                    }
                }

                // =========================
                // 🔥 SERIES
                // =========================
                var series = await dbHandler.GetSeriesById(seriesUid);
                if (series == null)
                {
                    series = new Series
                    {
                        SeriesInstanceUid = seriesUid,
                        StudyInstanceUid = studyUid,
                        PatientId = patientId,

                        Modality = ds.GetSingleValueOrDefault(DicomTag.Modality, ""),
                        SeriesNumber = ds.GetSingleValueOrDefault(DicomTag.SeriesNumber, 0),
                        SeriesDate = ds.GetSingleValueOrDefault(DicomTag.SeriesDate, DateTime.MinValue),
                        SeriesTime = ds.GetSingleValueOrDefault(DicomTag.SeriesTime, DateTime.MinValue),
                        SeriesDescription = ds.GetSingleValueOrDefault(DicomTag.SeriesDescription, ""),
                        BodyPartExamined = ds.GetSingleValueOrDefault(DicomTag.BodyPartExamined, ""),
                        ProtocolName = ds.GetSingleValueOrDefault(DicomTag.ProtocolName, ""),
                        PatientPosition = ds.GetSingleValueOrDefault(DicomTag.PatientPosition, ""),
                        NumberOfSeriesRelatedInstances = ds.GetSingleValueOrDefault(DicomTag.NumberOfSeriesRelatedInstances, 0),
                        SendingAETitle = ds.GetSingleValueOrDefault(DicomTag.SourceApplicationEntityTitle, "")
                    };

                    try
                    {
                        await dbHandler.AddSeries(series);
                    }
                    catch
                    {
                        // ignore duplicate
                    }
                }

                // =========================
                // 🔥 SAVE FILE (safe)
                // =========================
                using (var fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    await dicomFile.SaveAsync(fs);
                }

                // =========================
                // 🔥 IMAGE INSERT (FINAL STEP)
                // =========================
                double[] imagePosition = null;
                double[] imageOrientation = null;
                double[] pixelSpacing = null;
                string[] imageType = null;

                ds.TryGetValues(DicomTag.ImagePositionPatient, out imagePosition);
                ds.TryGetValues(DicomTag.ImageOrientationPatient, out imageOrientation);
                ds.TryGetValues(DicomTag.PixelSpacing, out pixelSpacing);
                ds.TryGetValues(DicomTag.ImageType, out imageType);

                var image = new Image
                {
                    SopInstanceUid = sopUid,
                    SeriesInstanceUid = seriesUid,
                    StudyInstanceUid = studyUid,
                    PatientId = patientId,
                    FilePath = filePath,

                    SopClassUid = ds.GetSingleValueOrDefault(DicomTag.SOPClassUID, ""),
                    TransferSyntaxUid = ds.InternalTransferSyntax?.UID.UID,
                    InstanceNumber = ds.GetSingleValueOrDefault(DicomTag.InstanceNumber, 0),

                    Rows = ds.GetSingleValueOrDefault(DicomTag.Rows, 0),
                    Columns = ds.GetSingleValueOrDefault(DicomTag.Columns, 0),

                    BitsAllocated = ds.GetSingleValueOrDefault(DicomTag.BitsAllocated, 0),
                    BitsStored = ds.GetSingleValueOrDefault(DicomTag.BitsStored, 0),
                    HighBit = ds.GetSingleValueOrDefault(DicomTag.HighBit, 0),
                    PixelRepresentation = ds.GetSingleValueOrDefault(DicomTag.PixelRepresentation, 0),

                    PhotometricInterpretation = ds.GetSingleValueOrDefault(DicomTag.PhotometricInterpretation, ""),
                    SamplesPerPixel = ds.GetSingleValueOrDefault(DicomTag.SamplesPerPixel, 0),

                    // ✅ SAFE handling
                    ImagePositionPatient = imagePosition != null ? string.Join("\\", imagePosition) : "",
                    ImageOrientationPatient = imageOrientation != null ? string.Join("\\", imageOrientation) : "",
                    PixelSpacing = pixelSpacing != null ? string.Join("\\", pixelSpacing) : "",

                    SliceThickness = ds.GetSingleValueOrDefault(DicomTag.SliceThickness, "").ToString(),
                    FrameOfReferenceUid = ds.GetSingleValueOrDefault(DicomTag.FrameOfReferenceUID, ""),

                    RescaleSlope = ds.GetSingleValueOrDefault(DicomTag.RescaleSlope, 1.0),
                    RescaleIntercept = ds.GetSingleValueOrDefault(DicomTag.RescaleIntercept, 0.0),

                    Kvp = ds.GetSingleValueOrDefault(DicomTag.KVP, ""),
                    XrayTubeCurrent = ds.GetSingleValueOrDefault(DicomTag.XRayTubeCurrent, ""),

                    EchoTime = ds.GetSingleValueOrDefault(DicomTag.EchoTime, ""),
                    RepetitionTime = ds.GetSingleValueOrDefault(DicomTag.RepetitionTime, ""),
                    FlipAngle = ds.GetSingleValueOrDefault(DicomTag.FlipAngle, ""),

                    AcquisitionTime = ds.GetSingleValueOrDefault(DicomTag.AcquisitionTime, DateTime.MinValue),
                    FrameCount = ds.GetSingleValueOrDefault(DicomTag.NumberOfFrames, 0),

                    ImageType = imageType != null ? string.Join("\\", imageType) : "",
                    ConvolutionKernel = ds.GetSingleValueOrDefault(DicomTag.ConvolutionKernel, "")
                };
                try
                {
                    await dbHandler.AddImage(image);
                    await dbHandler.SaveAsync(); // ✅ save only on success
                }
                catch
                {
                    // duplicate insert → safe ignore
                    if (File.Exists(filePath))
                        File.Delete(filePath);

                    return sopUid;
                }

                return sopUid;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    File.Delete(filePath);
                Console.Error.WriteLine($"Failed to process DICOM stream: {ex}");

                throw;
            }
        }

        public async Task DeleteAll(string userId)
        {
            var context = _userManager.GetUserDbContext(userId);
            var dbHandler = new DBHandler(context);
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Step 1: Get only file paths (NOT full images)
                var filePaths = await dbHandler.GetAllImagePaths();

                // Step 2: Delete files safely
                foreach (var path in filePaths)
                {
                    try
                    {
                        if (File.Exists(path))
                            File.Delete(path);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to delete file {path}: {ex.Message}");
                    }
                }

                // Step 3: Bulk delete (EF Core 7+ 🚀)
                await dbHandler.DeleteAllImages();
                await dbHandler.DeleteAllSeries();
                await dbHandler.DeleteAllStudies();
                await dbHandler.DeleteAllPatients();
                await dbHandler.SaveAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<List<StudyView>> GetAllStudies(string userId)
        {
            var context = _userManager.GetUserDbContext(userId);
            var dbHandler = new DBHandler(context);
            return await dbHandler.GetAllStudyView();
        }
        public async Task<List<ImageView>> GetImages(string studyUid, string userId)
        {
            var context = _userManager.GetUserDbContext(userId);
            var dbHandler = new DBHandler(context);
            return await dbHandler.GetImageViews(studyUid);
        }
        public async Task<FileStream?> GetImageFile(string sopUid, string userId)
        {
            var context = _userManager.GetUserDbContext(userId);
            var dbHandler = new DBHandler(context);
            var filePath = await dbHandler.GetImagePath(sopUid);

            if (filePath == null || !File.Exists(filePath))
                return null;

            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
