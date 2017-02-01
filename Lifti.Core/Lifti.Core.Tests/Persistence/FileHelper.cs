namespace Lifti.Tests.Persistence.DataFileManagerTests
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using PCLStorage;

    using FileAccess = PCLStorage.FileAccess;

    internal static class FileHelper
    {
        public static async Task DeleteIfExistsAsync(string fileName)
        {
            var localFolder = FileSystem.Current.LocalStorage;
            if ((await localFolder.CheckExistsAsync(fileName)) == ExistenceCheckResult.FileExists)
            {
                var file = await localFolder.GetFileAsync(fileName);
                await file.DeleteAsync();
            }
        }

        public static void CreateFile(string fileName, string content)
        {
            var localFolder = FileSystem.Current.LocalStorage;

            using (var writer = File.CreateText(Path.Combine(localFolder.Path, fileName)))
            {
                writer.Write("Test");
            }
        }

        public static byte[] GetFileBytes(string fileName)
        {
            var localFolder = FileSystem.Current.LocalStorage;
            return File.ReadAllBytes(Path.Combine(localFolder.Path, fileName));
        }

        public static void CreateFile(string fileName, byte[] bytes)
        {
            var localFolder = FileSystem.Current.LocalStorage;

            using (var writer = File.Create(Path.Combine(localFolder.Path, fileName)))
            {
                writer.Write(bytes, 0, bytes.Length);
            }
        }
    }
}