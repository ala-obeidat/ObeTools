using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

using ObeTools.Model;

using Xunit;

namespace ObeTools.Tests
{
    public class FileHelperTests
    {
        private static byte[] Bytes(params int[] values) => values.Select(v => (byte)v).Concat(new byte[32]).ToArray();
        private static byte[] Ascii(string s, int pad = 32) => Encoding.ASCII.GetBytes(s).Concat(new byte[pad]).ToArray();
        private static byte[] Box(string type, string brand) =>
            new byte[] { 0, 0, 0, 0x18 }.Concat(Encoding.ASCII.GetBytes(type + brand)).Concat(new byte[32]).ToArray();

        public static TheoryData<byte[], string, FileInfoType> Signatures => new TheoryData<byte[], string, FileInfoType>
        {
            { Bytes(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A), "png", FileInfoType.Image },
            { Bytes(0xFF, 0xD8, 0xFF, 0xE0), "jpg", FileInfoType.Image },
            { Ascii("GIF89a"), "gif", FileInfoType.Image },
            { Ascii("RIFF\x10\0\0\0WEBPVP8 "), "webp", FileInfoType.Image },
            { Box("ftyp", "heic"), "heic", FileInfoType.Image },
            { Ascii("%PDF-1.7"), "pdf", FileInfoType.Pdf },
            { Ascii("ID3\x04"), "mp3", FileInfoType.Audio },
            { Bytes(0xFF, 0xFB, 0x90), "mp3", FileInfoType.Audio },
            { Ascii("RIFF\x10\0\0\0WAVEfmt "), "wav", FileInfoType.Audio },
            { Box("ftyp", "isom"), "mp4", FileInfoType.Video },
            { Box("ftyp", "qt  "), "mov", FileInfoType.Video },
            { Bytes(0x1A, 0x45, 0xDF, 0xA3), "mkv", FileInfoType.Video },
            { Ascii("just some text"), "", FileInfoType.Unknown },
        };

        [Theory]
        [MemberData(nameof(Signatures))]
        public void DetectsBySignature(byte[] content, string extension, FileInfoType type)
        {
            var base64 = Convert.ToBase64String(content);

            Assert.Equal(extension, FileHelper.GetExtension(base64));
            Assert.Equal(extension, FileHelper.GetExtenstion(base64));
            var info = FileHelper.GetAllInfo(base64);
            Assert.Equal(extension, info.Extension);
            Assert.Equal(extension, info.Extenstion);
            Assert.Equal(type, info.Type);
            Assert.Equal(content.Length, info.SizeInBytes);
            Assert.Equal(content.Length, info.Size);
        }

        private static byte[] Zip(params string[] entries)
        {
            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var e in entries)
                {
                    using var w = new StreamWriter(zip.CreateEntry(e).Open());
                    w.Write("x");
                }
            }
            return ms.ToArray();
        }

        [Theory]
        [InlineData("docx", FileInfoType.Word, "[Content_Types].xml", "word/document.xml")]
        [InlineData("xlsx", FileInfoType.Spreadsheet, "[Content_Types].xml", "xl/workbook.xml")]
        [InlineData("pptx", FileInfoType.Presentation, "[Content_Types].xml", "ppt/presentation.xml")]
        [InlineData("apk", FileInfoType.Archive, "AndroidManifest.xml", "classes.dex")]
        [InlineData("zip", FileInfoType.Archive, "readme.txt")]
        public void ZipContainers_AreToldApart(string extension, FileInfoType type, params string[] entries)
        {
            var base64 = Convert.ToBase64String(Zip(entries));

            Assert.Equal(extension, FileHelper.GetExtension(base64));
            Assert.Equal(type, FileHelper.GetAllInfo(base64).Type);
        }

        [Fact]
        public void DataUriPrefix_IsAccepted()
        {
            var base64 = "data:application/pdf;base64," + Convert.ToBase64String(Ascii("%PDF-1.7"));
            Assert.Equal("pdf", FileHelper.GetExtension(base64));
            Assert.Equal("application/pdf", FileHelper.GetAllInfo(base64).MimeType);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("ab")]
        [InlineData("JVBE")]
        [InlineData("####")]
        public void ShortOrInvalidInput_DoesNotThrow(string input)
        {
            var extension = FileHelper.GetExtension(input);
            Assert.NotNull(extension);
        }

        [Fact]
        public void GetAllInfo_InvalidBase64_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => FileHelper.GetAllInfo("not base64!"));
        }

        [Fact]
        public void UnknownAlias_HasTheSameValue()
        {
            Assert.Equal(FileInfoType.Unknown, FileInfoType.Unkown);
        }
    }
}
