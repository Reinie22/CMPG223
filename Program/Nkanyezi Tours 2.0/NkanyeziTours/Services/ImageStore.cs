using System.IO;
using System.Web;
using System.Web.UI.WebControls;

namespace Nkanyezi.Services;

public static class ImageStore
{
    public const int MaxBytes = 5 * 1024 * 1024;

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public static bool TrySave(FileUpload upload, string folder, out string? url, out string? error)
    {
        url = null;
        error = null;

        if (upload == null || !upload.HasFile)
            return true;

        var extension = Path.GetExtension(upload.FileName ?? "").ToLowerInvariant();

        if (Array.IndexOf(AllowedExtensions, extension) < 0)
        {
            error = "The photo must be a JPG, PNG, GIF or WEBP file.";
            return false;
        }

        if (upload.PostedFile.ContentLength > MaxBytes)
        {
            error = "The photo must be 5 MB or smaller.";
            return false;
        }

        var bytes = upload.FileBytes;

        if (!LooksLikeImage(bytes))
        {
            error = "That file is not a valid image.";
            return false;
        }

        var relativeFolder = "~/Uploads/" + folder + "/";
        var physicalFolder = HttpContext.Current.Server.MapPath(relativeFolder);

        Directory.CreateDirectory(physicalFolder);

        var fileName = Guid.NewGuid().ToString("N") + extension;
        File.WriteAllBytes(Path.Combine(physicalFolder, fileName), bytes);

        url = relativeFolder + fileName;
        return true;
    }

    public static void Delete(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)
            || !url!.StartsWith("~/Uploads/", StringComparison.OrdinalIgnoreCase))
            return;

        try
        {
            var path = HttpContext.Current.Server.MapPath(url);

            if (File.Exists(path))
                File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static bool LooksLikeImage(byte[] b)
    {
        if (b == null || b.Length < 12)
            return false;

        var jpeg = b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF;
        var png  = b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47;
        var gif  = b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x38;
        var webp = b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46
                && b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50;

        return jpeg || png || gif || webp;
    }
}
