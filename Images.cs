using Microsoft.AspNetCore.Mvc;

namespace LionsBlog;

public class Images
{
    static public Task<IActionResult> GetImage(string imagepath)
    {
        var filestream = System.IO.File.OpenRead(imagepath);

        return Task.FromResult<IActionResult>(new FileStreamResult(filestream,"application/octet-stream"));
    }
}