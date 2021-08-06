using System;
using System.Collections.Generic;
using TitanBlog.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace TitanBlog.Services
{
    public class BasicImageService : IImageService
    {
        public string ContentType(IFormFile image)
        {
            return image?.ContentType;
        }

        public string DecodeImage(byte[] data, string type)
        {
            if (data is null || type is null) return null;
            return $"data:image/{type};base64,{Convert.ToBase64String(data)}";
        }

        public async Task<byte[]> EncodeImageAsync(IFormFile image)
        {
            if (image is null) return null;
            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            return ms.ToArray();
        }

        public async Task<byte[]> EncodeImageAsync(string fileName)
        {
           var imagePath = $"{Directory.GetCurrentDirectory()}/wwwroot/images/{fileName}";
            return await File.ReadAllBytesAsync(imagePath);

        }

        public int Size(IFormFile image)
        {
            return Convert.ToInt32(image?.Length);
        }
    }
}
