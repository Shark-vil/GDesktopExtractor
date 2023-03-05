using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GarrysmodDesktopAddonExtractor.Services
{
    public class CacheService
    {
        public static string? GetFileInCachePath(string filePath)
        {
            string? fileName = CalculateFileMD5Hash(filePath);
            if (fileName == null) return null;
            return Path.Combine(GetCacheDirectoryPath(), fileName + ".gma");
        }

        private static string? CalculateFileMD5Hash(string filePath)
        {
            byte[]? fileHashBytes = null;

            using (var md5 = MD5.Create())
                using (var stream = File.OpenRead(filePath))
                    fileHashBytes = md5.ComputeHash(stream);

            if (fileHashBytes == null)
                return null;

            return BitConverter.ToString(fileHashBytes).Replace("-", "").ToLowerInvariant();
        }

        private static string GetCacheDirectoryPath()
        {
            string cacheDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache/gma");

            if (!Directory.Exists(cacheDirectory))
                Directory.CreateDirectory(cacheDirectory);

            return cacheDirectory;
        }
    }
}
