using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MyLeasing.Web.Helpers
{
	public interface IImageHelper
	{
		//subir foto
		Task<string> UploadImageAsync(IFormFile imageFile);
	}
}

