namespace PacsApi.DataManagement
{
    public class FileHandler : IFileHandler
    {
        public void DeleteDicomFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        public async Task<string> SaveDicomFile(byte[] data, string folderPath, string fileName)
        {
            Directory.CreateDirectory(folderPath);
            string fullPath = Path.Combine(folderPath, fileName);
            await File.WriteAllBytesAsync(fullPath, data);
            return fullPath;
        }
    }
}
