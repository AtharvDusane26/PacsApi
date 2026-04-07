using EFCore.lib.Utility;
using FellowOakDicom;
using Logging;
using Microsoft.EntityFrameworkCore.Internal;
using PacsApi.Authentication;
using PacsApi.Context;
using PacsApi.DataManagement;
using PacsApi.DTO;
using PacsApi.Models;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using LogLevel = Logging.LogLevel;

namespace PacsApi.Services.Import
{
    public class ImportService : Service
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _userLocks = new();
        private readonly PacsDbContextFactory _contextFactory;

        public ImportService(
            PacsDbContextFactory contextFactory,
            LoggerService logger,
            IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory, logger)
        {
            _contextFactory = contextFactory;
        }

        public async Task Upload(string userId, List<IFormFile> files)
        {
            var userLock = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
            await userLock.WaitAsync();

            try
            {
                var streams = await ConvertToStreams(files);

                foreach (var stream in streams)
                {
                    try
                    {
                        await Import(stream, userId);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, $"Error processing DICOM stream for user {userId}: {ex.Message}");
                    }
                    finally
                    {
                        stream.Dispose(); // 🔥 prevent memory leak
                    }
                }
            }
            finally
            {
                userLock.Release();
            }
        }

        private async Task<List<Stream>> ConvertToStreams(List<IFormFile> files)
        {
            var streams = new ConcurrentBag<Stream>();

            await Parallel.ForEachAsync(files, async (file, ct) =>
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms, ct);

                var bytes = ms.ToArray();
                streams.Add(new MemoryStream(bytes)); // safe independent stream
            });

            return streams.ToList();
        }

        public async Task UploadSingle(string userId, IFormFile file)
        {
            var userLock = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
            await userLock.WaitAsync();

            try
            {
                if (file == null)
                {
                    Logger.Log(LogLevel.Error, $"User {userId} attempted to upload a null file.");
                    throw new Exception("No file uploaded");
                }

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                var bytes = ms.ToArray();
                using var stream = new MemoryStream(bytes); // 🔥 ensure disposal

                await Import(stream, userId);
            }
            finally
            {
                userLock.Release();
            }
        }

        private async Task<string> Import(Stream dicomStream, string userId)
        {
            string filePath = string.Empty;

            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            using var unitOfWork = Init(context);

            var dbHandler = new DBHandler(unitOfWork, context);

            await dbHandler.BeginTransactionAsync();

            try
            {
                var dicomFile = await DicomFile.OpenAsync(dicomStream, FileReadOption.Default);
                var ds = dicomFile.Dataset;

                string patientId = ds.GetSingleValueOrDefault(DicomTag.PatientID, "");
                string studyUid = ds.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, "");
                string seriesUid = ds.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, "");
                string sopUid = ds.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, "");

                string root = Path.Combine(
                    GeneralSettings.BaseDirectory,
                    "Images",
                    patientId,
                    studyUid,
                    seriesUid);

                Directory.CreateDirectory(root);

                filePath = Path.Combine(root, $"{sopUid}.dic");

                if (File.Exists(filePath))
                    File.Delete(filePath);

                await using (var fs = new FileStream(
                    filePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    await dicomFile.SaveAsync(fs);
                }

                if (await dbHandler.ImageExist(sopUid))
                {
                    await dbHandler.RollbackAsync();
                    return sopUid;
                }

                // ================= PATIENT =================
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
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, $"Duplicate patient insert for {patientId} - {ex.Message}");
                    }
                }

                // ================= STUDY =================
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
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, $"Duplicate study insert for {studyUid} - {ex.Message}");
                    }
                }

                // ================= SERIES =================
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
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, $"Duplicate series insert for {seriesUid} - {ex.Message}");
                    }
                }

                // ================= IMAGE =================
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
                    await dbHandler.CommitAsync();
                }
                catch (Exception ex)
                {
                    await dbHandler.RollbackAsync();

                    Logger.Log(LogLevel.Error, $"Failed image insert {sopUid} - {ex.Message}");

                    if (File.Exists(filePath))
                        File.Delete(filePath);

                    return sopUid;
                }

                return sopUid;
            }
            catch (Exception ex)
            {
                await dbHandler.RollbackAsync();

                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    File.Delete(filePath);

                Logger.Log(LogLevel.Error, $"Failed to process DICOM stream: {ex}");
                throw;
            }
        }
    }
}
