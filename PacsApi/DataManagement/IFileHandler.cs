namespace PacsApi.DataManagement
{
    public interface IFileHandler
    {
        Task<string> SaveDicomFile(byte[] dicomData, string folderPath, string fileName);
        void DeleteDicomFile(string filePath);
    }
}
