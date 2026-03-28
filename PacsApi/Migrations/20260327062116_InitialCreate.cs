using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PacsApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientId = table.Column<string>(type: "TEXT", nullable: false),
                    PatientName = table.Column<string>(type: "TEXT", nullable: false),
                    PatientSex = table.Column<string>(type: "TEXT", nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: true),
                    AgeString = table.Column<string>(type: "TEXT", nullable: false),
                    PatientBirthDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExaminingDoctorId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientId);
                });

            migrationBuilder.CreateTable(
                name: "Studies",
                columns: table => new
                {
                    StudyInstanceUid = table.Column<string>(type: "TEXT", nullable: false),
                    PatientId = table.Column<string>(type: "TEXT", nullable: false),
                    StudyDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StudyTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StudyId = table.Column<string>(type: "TEXT", nullable: true),
                    AccessionNumber = table.Column<string>(type: "TEXT", nullable: false),
                    StudyDescription = table.Column<string>(type: "TEXT", nullable: false),
                    ReferringPhysicianName = table.Column<string>(type: "TEXT", nullable: false),
                    PerformingPhysician = table.Column<string>(type: "TEXT", nullable: false),
                    InstitutionName = table.Column<string>(type: "TEXT", nullable: false),
                    ExaminingDoctorId = table.Column<int>(type: "INTEGER", nullable: true),
                    NumberOfStudyRelatedInstances = table.Column<int>(type: "INTEGER", nullable: true),
                    NumberOfStudyRelatedSeries = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDicomSent = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsAutoRouted = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsPrinted = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsCdWritten = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: true),
                    Report = table.Column<string>(type: "TEXT", nullable: true),
                    ReportStatus = table.Column<string>(type: "TEXT", nullable: true),
                    SendingAETitle = table.Column<string>(type: "TEXT", nullable: true),
                    ReceivingDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PatientWeight = table.Column<double>(type: "REAL", nullable: true),
                    PatientSize = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Studies", x => x.StudyInstanceUid);
                    table.ForeignKey(
                        name: "FK_Studies_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Series",
                columns: table => new
                {
                    SeriesInstanceUid = table.Column<string>(type: "TEXT", nullable: false),
                    StudyInstanceUid = table.Column<string>(type: "TEXT", nullable: false),
                    PatientId = table.Column<string>(type: "TEXT", nullable: false),
                    Modality = table.Column<string>(type: "TEXT", nullable: false),
                    SeriesNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    SeriesDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SeriesTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SeriesDescription = table.Column<string>(type: "TEXT", nullable: false),
                    BodyPartExamined = table.Column<string>(type: "TEXT", nullable: false),
                    ProtocolName = table.Column<string>(type: "TEXT", nullable: false),
                    PatientPosition = table.Column<string>(type: "TEXT", nullable: false),
                    SendingAETitle = table.Column<string>(type: "TEXT", nullable: false),
                    NumberOfSeriesRelatedInstances = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Series", x => x.SeriesInstanceUid);
                    table.ForeignKey(
                        name: "FK_Series_Studies_StudyInstanceUid",
                        column: x => x.StudyInstanceUid,
                        principalTable: "Studies",
                        principalColumn: "StudyInstanceUid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    SopInstanceUid = table.Column<string>(type: "TEXT", nullable: false),
                    SeriesInstanceUid = table.Column<string>(type: "TEXT", nullable: false),
                    StudyInstanceUid = table.Column<string>(type: "TEXT", nullable: false),
                    PatientId = table.Column<string>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    SopClassUid = table.Column<string>(type: "TEXT", nullable: false),
                    TransferSyntaxUid = table.Column<string>(type: "TEXT", nullable: false),
                    InstanceNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    Rows = table.Column<int>(type: "INTEGER", nullable: true),
                    Columns = table.Column<int>(type: "INTEGER", nullable: true),
                    BitsAllocated = table.Column<int>(type: "INTEGER", nullable: true),
                    BitsStored = table.Column<int>(type: "INTEGER", nullable: true),
                    HighBit = table.Column<int>(type: "INTEGER", nullable: true),
                    PixelRepresentation = table.Column<int>(type: "INTEGER", nullable: true),
                    PhotometricInterpretation = table.Column<string>(type: "TEXT", nullable: false),
                    SamplesPerPixel = table.Column<int>(type: "INTEGER", nullable: true),
                    ImagePositionPatient = table.Column<string>(type: "TEXT", nullable: false),
                    ImageOrientationPatient = table.Column<string>(type: "TEXT", nullable: false),
                    PixelSpacing = table.Column<string>(type: "TEXT", nullable: false),
                    SliceThickness = table.Column<string>(type: "TEXT", nullable: false),
                    FrameOfReferenceUid = table.Column<string>(type: "TEXT", nullable: false),
                    Modality = table.Column<string>(type: "TEXT", nullable: true),
                    BodyPartExamined = table.Column<string>(type: "TEXT", nullable: true),
                    ProtocolName = table.Column<string>(type: "TEXT", nullable: true),
                    AcquisitionTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RescaleSlope = table.Column<double>(type: "REAL", nullable: true),
                    RescaleIntercept = table.Column<double>(type: "REAL", nullable: true),
                    Kvp = table.Column<string>(type: "TEXT", nullable: false),
                    XrayTubeCurrent = table.Column<string>(type: "TEXT", nullable: false),
                    EchoTime = table.Column<string>(type: "TEXT", nullable: false),
                    RepetitionTime = table.Column<string>(type: "TEXT", nullable: false),
                    FlipAngle = table.Column<string>(type: "TEXT", nullable: false),
                    FrameCount = table.Column<int>(type: "INTEGER", nullable: true),
                    FrameTime = table.Column<string>(type: "TEXT", nullable: true),
                    CineRate = table.Column<string>(type: "TEXT", nullable: true),
                    ImageType = table.Column<string>(type: "TEXT", nullable: false),
                    ConvolutionKernel = table.Column<string>(type: "TEXT", nullable: false),
                    AccessionNumber = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.SopInstanceUid);
                    table.ForeignKey(
                        name: "FK_Images_Series_SeriesInstanceUid",
                        column: x => x.SeriesInstanceUid,
                        principalTable: "Series",
                        principalColumn: "SeriesInstanceUid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Images_SeriesInstanceUid_InstanceNumber",
                table: "Images",
                columns: new[] { "SeriesInstanceUid", "InstanceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Images_SopInstanceUid",
                table: "Images",
                column: "SopInstanceUid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_StudyInstanceUid",
                table: "Images",
                column: "StudyInstanceUid");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_PatientId",
                table: "Patients",
                column: "PatientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Series_SeriesInstanceUid",
                table: "Series",
                column: "SeriesInstanceUid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Series_StudyInstanceUid",
                table: "Series",
                column: "StudyInstanceUid");

            migrationBuilder.CreateIndex(
                name: "IX_Studies_PatientId",
                table: "Studies",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Studies_StudyInstanceUid",
                table: "Studies",
                column: "StudyInstanceUid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "Series");

            migrationBuilder.DropTable(
                name: "Studies");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
