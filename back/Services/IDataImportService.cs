namespace back.Services
{
    public interface IDataImportService
    {
        Task ImportData(string filePath);
    }
}