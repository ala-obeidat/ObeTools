using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

using ObeTools.Model;

namespace ObeTools
{
    /// <summary>
    /// Get File information from the file content (magic bytes), not from a file name.
    /// </summary>
    public static class FileHelper
    {
        #region Variable

        private const string OctetStream = "application/octet-stream";

        // Enough base64 for the longest signature we read (12 bytes).
        private const int HeaderBase64Length = 32;

        #endregion

        #region Method

        /// <summary>
        /// Get file extension from base64 (a "data:...;base64," prefix is allowed).
        /// ZIP files are inspected to tell docx, xlsx, pptx and apk apart; other ZIPs return "zip".
        /// </summary>
        /// <param name="fileBase64">File in Base64 encoding</param>
        /// <returns>extension without the dot, or empty when unknown or not base64</returns>
        public static string GetExtension(string fileBase64)
        {
            var base64 = CleanBase64(fileBase64);
            var header = DecodeHeader(base64);
            var result = Detect(header);
            if (result.Extension == "zip" && TryDecode(base64, out var bytes))
            {
                result = Detect(bytes);
            }
            return result.Extension;
        }

        /// <summary>
        /// Get file extenstion from base64. Misspelled alias of <see cref="GetExtension"/>.
        /// </summary>
        /// <param name="fileBase64">File in Base64 encoding</param>
        /// <returns>extenstion as string</returns>
        [Obsolete("Use GetExtension.")]
        public static string GetExtenstion(string fileBase64)
        {
            return GetExtension(fileBase64);
        }

        /// <summary>
        /// Get File information from base64 (a "data:...;base64," prefix is allowed).
        /// </summary>
        /// <param name="fileBase64">File in Base64 encoding</param>
        /// <returns>File information as object</returns>
        /// <exception cref="FormatException">The content is not valid base64.</exception>
        public static FileInformation GetAllInfo(string fileBase64)
        {
            return GetAllInfo(Convert.FromBase64String(CleanBase64(fileBase64)));
        }

        /// <summary>
        /// Get File information from the file bytes.
        /// </summary>
        /// <param name="fileBytes">File content</param>
        /// <returns>File information as object</returns>
        public static FileInformation GetAllInfo(byte[] fileBytes)
        {
            fileBytes ??= Array.Empty<byte>();
            var (extension, mimeType, type) = Detect(fileBytes);
            return new FileInformation
            {
                FileBytes = fileBytes,
                Extension = extension,
                MimeType = mimeType,
                Type = type,
                Size = fileBytes.LongLength,
                SizeInBytes = fileBytes.LongLength
            };
        }

        #endregion

        #region Helper

        private static string CleanBase64(string fileBase64)
        {
            if (string.IsNullOrEmpty(fileBase64))
            {
                return string.Empty;
            }
            int marker = fileBase64.IndexOf("base64,", StringComparison.Ordinal);
            if (marker >= 0)
            {
                fileBase64 = fileBase64.Substring(marker + "base64,".Length);
            }
            return fileBase64.Trim();
        }

        private static byte[] DecodeHeader(string base64)
        {
            var head = new string(base64.Where(c => !char.IsWhiteSpace(c)).Take(HeaderBase64Length).ToArray());
            head = head.Substring(0, head.Length - (head.Length % 4));
            try
            {
                return Convert.FromBase64String(head);
            }
            catch (FormatException)
            {
                return Array.Empty<byte>();
            }
        }

        private static bool TryDecode(string base64, out byte[] bytes)
        {
            try
            {
                bytes = Convert.FromBase64String(base64);
                return true;
            }
            catch (FormatException)
            {
                bytes = null;
                return false;
            }
        }

        private static (string Extension, string MimeType, FileInfoType Type) Detect(byte[] b)
        {
            if (StartsWith(b, 0, 0x89, 0x50, 0x4E, 0x47))
                return ("png", "image/png", FileInfoType.Image);
            if (StartsWith(b, 0, 0xFF, 0xD8, 0xFF))
                return ("jpg", "image/jpeg", FileInfoType.Image);
            if (Ascii(b, 0, "GIF8"))
                return ("gif", "image/gif", FileInfoType.Image);
            if (Ascii(b, 0, "%PDF"))
                return ("pdf", "application/pdf", FileInfoType.Pdf);
            if (Ascii(b, 0, "RIFF"))
            {
                if (Ascii(b, 8, "WEBP"))
                    return ("webp", "image/webp", FileInfoType.Image);
                if (Ascii(b, 8, "WAVE"))
                    return ("wav", "audio/wav", FileInfoType.Audio);
                if (Ascii(b, 8, "AVI "))
                    return ("avi", "video/x-msvideo", FileInfoType.Video);
                return Unknown();
            }
            if (Ascii(b, 0, "ID3") || StartsWith(b, 0, 0xFF, 0xFB) || StartsWith(b, 0, 0xFF, 0xF3) || StartsWith(b, 0, 0xFF, 0xF2))
                return ("mp3", "audio/mpeg", FileInfoType.Audio);
            if (StartsWith(b, 0, 0x1A, 0x45, 0xDF, 0xA3))
                return ("mkv", "video/x-matroska", FileInfoType.Video);
            if (StartsWith(b, 0, 0x50, 0x4B, 0x03, 0x04) || StartsWith(b, 0, 0x50, 0x4B, 0x05, 0x06))
                return DetectZip(b);
            if (Ascii(b, 4, "ftyp"))
                return DetectIsoMedia(b);
            if (StartsWith(b, 0, 0x00, 0x00, 0x00)
                && (Ascii(b, 4, "moov") || Ascii(b, 4, "mdat") || Ascii(b, 4, "wide") || Ascii(b, 4, "free") || Ascii(b, 4, "pnot")))
                return ("mov", "video/quicktime", FileInfoType.Video);

            return Unknown();
        }

        private static (string, string, FileInfoType) DetectIsoMedia(byte[] b)
        {
            if (b.Length < 12)
                return ("mp4", "video/mp4", FileInfoType.Video);

            var brand = Encoding.ASCII.GetString(b, 8, 4);
            switch (brand)
            {
                case "qt  ":
                    return ("mov", "video/quicktime", FileInfoType.Video);
                case "heic":
                case "heix":
                case "hevc":
                case "hevx":
                case "heim":
                case "heis":
                case "mif1":
                case "msf1":
                    return ("heic", "image/heic", FileInfoType.Image);
                case "avif":
                case "avis":
                    return ("avif", "image/avif", FileInfoType.Image);
                case "M4A ":
                    return ("m4a", "audio/mp4", FileInfoType.Audio);
                default:
                    if (brand.StartsWith("3g", StringComparison.Ordinal))
                        return ("3gp", "video/3gpp", FileInfoType.Video);
                    return ("mp4", "video/mp4", FileInfoType.Video);
            }
        }

        // With the whole file we can look inside; with only the header we can only say "zip".
        private static (string, string, FileInfoType) DetectZip(byte[] b)
        {
            var zip = ("zip", "application/zip", FileInfoType.Archive);
            try
            {
                using var archive = new ZipArchive(new MemoryStream(b, false), ZipArchiveMode.Read);
                bool HasEntry(string prefix) => archive.Entries.Any(e => e.FullName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

                if (HasEntry("[Content_Types].xml"))
                {
                    if (HasEntry("word/"))
                        return ("docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", FileInfoType.Word);
                    if (HasEntry("xl/"))
                        return ("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", FileInfoType.Spreadsheet);
                    if (HasEntry("ppt/"))
                        return ("pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation", FileInfoType.Presentation);
                }
                if (HasEntry("AndroidManifest.xml"))
                    return ("apk", "application/vnd.android.package-archive", FileInfoType.Archive);
                return zip;
            }
            catch (Exception)
            {
                return zip; // truncated or not really a ZIP
            }
        }

        private static (string, string, FileInfoType) Unknown()
        {
            return (string.Empty, OctetStream, FileInfoType.Unknown);
        }

        private static bool StartsWith(byte[] b, int offset, params byte[] signature)
        {
            if (b.Length < offset + signature.Length)
                return false;
            for (int i = 0; i < signature.Length; i++)
            {
                if (b[offset + i] != signature[i])
                    return false;
            }
            return true;
        }

        private static bool Ascii(byte[] b, int offset, string signature)
        {
            return StartsWith(b, offset, Encoding.ASCII.GetBytes(signature));
        }

        #endregion
    }
}
