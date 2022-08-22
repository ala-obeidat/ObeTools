using System;

using ObeTools.Model;

namespace ObeTools
{
    /// <summary>
    /// Get File information
    /// </summary>
    public static class FileHelper
    {
        #region Variable 
        #endregion

        #region Method

        /// <summary>
        /// Get file extenstion from base64
        /// </summary>
        /// <param name="fileBase64">File in Base64 encoding</param>
        /// <returns>extenstion as string</returns>
        public static string GetExtenstion(string fileBase64)
        {
            if (fileBase64.Contains("base64,"))
            {
                fileBase64 = fileBase64.Split("base64,")[1];
            }
            string extension = fileBase64[..4].ToUpper();
            switch (extension)
            {
                case "IVBO":
                    return "png";
                case "/9J/":
                    return "jpg";
                case "JVBE":
                    return "pdf";
                case "UESD":
                    return "docx";
                case "SUQZ":
                    return "mp3";
                case "UKLG":
                    return "wav";
                case "AAAA":
                    if (fileBase64[5].ToString().ToUpper() == "F")
                    {
                        return "mov";
                    }
                    else
                    {
                        return "mp4";
                    }

                case "GKXF":
                    return "mkv";
                default:
                    return string.Empty;
            }

        }

        /// <summary>
        /// Get File information from base64
        /// </summary>
        /// <param name="fileBase64">File in Base64 encoding</param>
        /// <returns>File information as object</returns>
        public static FileInformation GetAllInfo(string fileBase64)
        {
            var response = new FileInformation();
            if (fileBase64.Contains("base64,"))
            {
                fileBase64 = fileBase64.Split("base64,")[1];
            }
            string extension = fileBase64[..4].ToUpper();
            switch (extension)
            {
                case "IVBO":
                    response.Extenstion = "png";
                    response.Type = FileInfoType.Image;
                    break;

                case "/9J/":
                    response.Extenstion = "jpg";
                    response.Type = FileInfoType.Image;
                    break;
                case "JVBE":
                    response.Extenstion = "pdf";
                    response.Type = FileInfoType.Pdf;
                    break;
                case "UESD":
                    response.Extenstion = "docx";
                    response.Type = FileInfoType.Word;
                    break;
                case "SUQZ":
                    response.Extenstion = "mp3";
                    response.Type = FileInfoType.Audio;
                    break;
                case "UKLG":
                    response.Extenstion = "wav";
                    response.Type = FileInfoType.Audio;
                    break;
                case "AAAA":
                    if (fileBase64[5].ToString().ToUpper() == "F")
                    {
                        response.Extenstion = "mov";
                    }
                    else
                    {
                        response.Extenstion = "mp4";
                    }
                    response.Type = FileInfoType.Video;
                    break;
                case "GKXF":
                    response.Extenstion = "mkv";
                    response.Type = FileInfoType.Video;
                    break;
                default:
                    break;
            }
            response.FileBytes = Convert.FromBase64String(fileBase64);
            response.Size = response.FileBytes.LongLength;
            return response;
        }
        #endregion

        #region Helper

        #endregion
    }
}
