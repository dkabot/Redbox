using System;
using System.IO;
using Newtonsoft.Json;

namespace DeviceService.Domain
{
    public static class DataFileHelper
    {
        public static T GetFileData<T>(
            string dataFilePath,
            string fileName,
            Action<string, Exception> exceptionLogger)
        {
            var path = Path.Combine(dataFilePath, fileName);
            var fileData = default(T);
            if (File.Exists(path))
                try
                {
                    using (var streamReader = File.OpenText(path))
                    {
                        fileData = (T)new JsonSerializer().Deserialize(streamReader, typeof(T));
                    }
                }
                catch (JsonSerializationException ex)
                {
                    exceptionLogger("DataFileHelper.GetFileData - JSsonSerializationException", ex);
                }
                catch (Exception ex)
                {
                    exceptionLogger("DataFileHelper.GetFileData - Exception", ex);
                }

            return fileData;
        }

        public static bool TryUpdateFileData<T>(string dataFilePath, string fileName, T data)
        {
            try
            {
                if (!Directory.Exists(dataFilePath))
                    Directory.CreateDirectory(dataFilePath);
                File.WriteAllText(Path.Combine(dataFilePath, fileName), JsonConvert.SerializeObject(data));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}