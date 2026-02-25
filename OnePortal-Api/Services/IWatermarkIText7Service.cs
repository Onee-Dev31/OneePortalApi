namespace OnePortal_Api.Services
{
    public interface IWatermarkIText7Service
    {
        Task<string> AddWatermarkToPdf(IFormFile file, string watermarkText, string folderName);
    }
}
