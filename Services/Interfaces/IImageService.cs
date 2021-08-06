using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TitanBlog.Services.Interfaces
{
    interface IImageService
    {
        Task<byte[]> encodeImageAsync(IFormFile image);
        Task<byte[]> encodeImageAsync(string image);
        string decodeImage(byte[] data, string type);
        string ContentType(IFormFile image);
        int Size(IFormFile image);


    }
}
