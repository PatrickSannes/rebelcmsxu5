using System;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Umbraco.Framework;
using File = Umbraco.Framework.Persistence.Model.IO.File;

namespace Umbraco.Cms.Web
{
    public static class FileExtensions
    {
        public static string GetFilePathForDisplay(this File file)
        {
            return file == null ? string.Empty : string.Join("/", file.Id.Value.Value.ToString().Split('\\').Select(x => x.ToFileNameForDisplay()));
        }

        public static string GetFilePathWithoutExtension(this File file)
        {
            if (file == null) return string.Empty;
            var path = string.Join("/", file.Id.Value.Value.ToString().Split('\\'));
            return path.Contains(".") ? path.Substring(0, path.LastIndexOf(".")) : path;
        }

        /// <summary>
        /// Returns a nicely formatted file name: without extension and hyphens replaced by spaces for display
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFileNameForDisplay(this File file)
        {
            if (file == null) return string.Empty;
            return file.Name.ToFileNameForDisplay();
        }

        /// <summary>
        /// Returns the file name without extension
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFileNameWithoutExtension(this File file)
        {
            if (file == null) return string.Empty;
            return Path.GetFileNameWithoutExtension(file.Name);
        }

        /// <summary>
        /// Returns the file extension
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFileExtension(this File file)
        {
            if (file == null) return string.Empty;
            return Path.GetExtension(file.Name);
        }

        public static string GetMimeType(this File file)
        {
            if (file != null)
            {
                var extension = Path.GetExtension(file.Name);
                var fileExtension = extension.IsNullOrWhiteSpace() ? file.Name : extension.ToLower();
                var rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(fileExtension);
                if (rk != null && rk.GetValue("Content Type") != null) return rk.GetValue("Content Type").ToString();
            }
            return "application/octet-stream";
        }

        public static bool IsImage(this File file)
        {
            if (file == null) return false;
            return (",.jpeg,.jpg,.gif,.bmp,.png,.tiff,.tif,")
                .IndexOf("," + Path.GetExtension(file.Name) + ",", StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        private static string ToFileNameForDisplay(this string fileName)
        {
            if (fileName.IsNullOrWhiteSpace())
                return "";

            fileName = Path.GetFileNameWithoutExtension(fileName) // Remove extension
                .TrimStart('_'); // Remove underscore prefix from private files

            fileName = Regex.Replace(fileName, "(?<![A-Z])([A-Z])", " $1", RegexOptions.Compiled).Trim(); // Insert space before upper case chars
            fileName = Regex.Replace(fileName, "([_-])", " ", RegexOptions.Compiled).Trim(); // Replace underscores / hyphens with spaces
            fileName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileName); // Convert to title case

            return fileName;
        }
    }
}
