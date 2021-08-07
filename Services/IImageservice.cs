using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TitanBlog.Services
{
    public interface IImageservice
    {
        Task<byte[]> EncodeImageAsync(IFormFile image);
        Task<byte[]> EncodeImageAsync(string image);
        string DecodeImage(byte[] data, string type);
        string ContentType(IFormFile image);
        int Size(IFormFile image);
    }
}