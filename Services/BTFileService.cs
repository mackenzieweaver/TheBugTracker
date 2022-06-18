using System;
using System.IO;
using System.Threading.Tasks;
using ByteSizeLib;
using Microsoft.AspNetCore.Http;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Services
{
    public class BTFileService : IBTFileService
    {
        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            if(fileData == null) return "";
            return $"data:{extension};base64,{Convert.ToBase64String(fileData)}";
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            MemoryStream ms = new();
            await file.CopyToAsync(ms);
            byte[] bytes = ms.ToArray();
            ms.Close();
            ms.Dispose();
            return bytes;
        }

        public string FormatFileSize(long bytes)
        {
            return ByteSize.FromBytes(bytes).ToString();
        }

        public string GetFileIcon(string file)
        {
            if(file is null) return "default";
            return $"/img/png/{Path.GetFileName(file)}";
        }
    }
}
