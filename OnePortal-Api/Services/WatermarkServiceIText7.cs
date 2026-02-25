using Microsoft.AspNetCore.Http;
using System.IO;

// iText
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Extgstate;

// ✅ กันชนชื่อ Path
using IOPath = System.IO.Path;

namespace OnePortal_Api.Services
{
    public class WatermarkServiceIText7 : IWatermarkIText7Service
    {
        private readonly string _wwwrootPath;

        public WatermarkServiceIText7()
        {
            _wwwrootPath = IOPath.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            if (!Directory.Exists(_wwwrootPath))
                Directory.CreateDirectory(_wwwrootPath);
        }

        // ✅ ชื่อต้องตรงกับ interface
        public async Task<string> AddWatermarkToPdf(IFormFile file, string watermarkText, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty.");

            folderName ??= "";

            string uploadPath = IOPath.Combine(_wwwrootPath, "uploads", folderName);
            Directory.CreateDirectory(uploadPath);

            string originalFileName = string.IsNullOrWhiteSpace(folderName)
                ? file.FileName
                : $"{Guid.NewGuid()}_{file.FileName}";

            string inputPath = IOPath.Combine(uploadPath, originalFileName);

            using (var fs = new FileStream(inputPath, FileMode.Create))
                await file.CopyToAsync(fs);

            string outputPath = string.IsNullOrWhiteSpace(folderName)
                ? inputPath
                : IOPath.Combine(uploadPath, $"watermarked_{Guid.NewGuid()}.pdf");

            using var reader = new PdfReader(inputPath);
            using var writer = new PdfWriter(outputPath);
            using var pdfDoc = new PdfDocument(reader, writer);

            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            var gs = new PdfExtGState()
                .SetFillOpacity(0.2f)
                .SetStrokeOpacity(0.2f)
                .SetBlendMode(PdfExtGState.BM_MULTIPLY);

            var fillColor = new DeviceRgb(180, 180, 180);
            var strokeColor = new DeviceRgb(255, 255, 255);

            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                var page = pdfDoc.GetPage(i);
                Rectangle ps = page.GetPageSize();

                float pageW = ps.GetWidth();
                float pageH = ps.GetHeight();
                float fontSize = Math.Min(pageW, pageH) / 5f;

                var canvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDoc);

                canvas.SaveState();
                canvas.SetExtGState(gs);
                canvas.SetFillColor(fillColor);
                canvas.SetStrokeColor(strokeColor);
                canvas.SetLineWidth(1.6f);

                float cx = ps.GetLeft() + pageW / 2f + 80f;
                float cy = ps.GetBottom() + pageH / 2f - 80f;

                double rad = 45 * Math.PI / 180;

                float maxTextWidth = pageW * 0.85f;
                float textWidth = font.GetWidth(watermarkText, fontSize);
                if (textWidth > maxTextWidth && textWidth > 0)
                {
                    fontSize *= (maxTextWidth / textWidth);
                    textWidth = font.GetWidth(watermarkText, fontSize);
                }

                canvas.BeginText();
                canvas.SetFontAndSize(font, fontSize);
                canvas.SetTextRenderingMode(PdfCanvasConstants.TextRenderingMode.FILL_STROKE);
                float textHeight = fontSize;

                canvas.SetTextMatrix(
                    (float)Math.Cos(rad), (float)Math.Sin(rad),
                    (float)-Math.Sin(rad), (float)Math.Cos(rad),
                    cx - textWidth / 2f,
                    cy - textHeight / 3f
                );

                canvas.ShowText(watermarkText);
                canvas.EndText();
                canvas.RestoreState();
            }

            return IOPath.Combine("uploads", folderName, IOPath.GetFileName(outputPath))
                .Replace("\\", "/");
        }
    }
}