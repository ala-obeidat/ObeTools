

using QRCoder;

using System.DrawingCore;
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
            using QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.H, utfEncoding, utfEncoding);
            using var qrCode = new QRCode(qrCodeData);
            using var qrCodeImage = qrCode.GetGraphic(5, darkColor: System.DrawingCore.Color.Black, System.DrawingCore.Color.White, icon: centerImage);
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
