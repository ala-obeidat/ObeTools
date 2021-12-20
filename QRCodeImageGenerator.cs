using QRCoder;

using System.Drawing;
using System.IO;
namespace ObeTools
{
    public static class QRCodeImageGenerator
    {
        #region Methods
        public static void Build(string text, string imagePath, string centerPath, bool utfEncoding)
        {
            Bitmap centerImage = null;
            if (!string.IsNullOrEmpty(centerPath) && File.Exists(centerPath))
            {
                centerImage = (Bitmap)Image.FromFile(centerPath);
            }

            using var qrGenerator = new QRCodeGenerator();
            using QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.H, utfEncoding, utfEncoding, utfEncoding ? QRCodeGenerator.EciMode.Utf8 : QRCodeGenerator.EciMode.Default);
            using var qrCode = new QRCode(qrCodeData);
            using var qrCodeImage = qrCode.GetGraphic(5, Color.Black, Color.White, centerImage);
            qrCodeImage.SetResolution(600, 600);
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
            qrCodeImage.Save(imagePath);
        }
        #endregion
    }
}
