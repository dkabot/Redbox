using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services
{
    public class PersistentDataCacheService : IPersistentDataCacheService
    {
        private const int _lockWait = 2000;
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();
        private readonly ILogger<PersistentDataCacheService> _logger;
        private readonly IStoreService _storeService;
        private string _dataCachePath;

        public PersistentDataCacheService(
            ILogger<PersistentDataCacheService> logger,
            IStoreService storeService)
        {
            _logger = logger;
            _storeService = storeService;
        }

        private string DataCachePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_dataCachePath))
                {
                    _dataCachePath = Path.GetFullPath(Path.Combine(_storeService.DataPath, "DataCache"));
                    Directory.CreateDirectory(_dataCachePath);
                }

                return _dataCachePath;
            }
        }

        public async Task<PersistentDataWrapper<T>> Read<T>(
            string fileName,
            bool useCache = false,
            string baseDirectory = null,
            bool log = true)
            where T : IPersistentData
        {
            var result = new PersistentDataWrapper<T>();
            try
            {
                if (await _lock.WaitAsync(2000))
                    try
                    {
                        result = await InnerRead<T>(fileName, useCache, baseDirectory, log);
                    }
                    finally
                    {
                        _lock.Release();
                    }
                else
                    _logger.LogErrorWithSource(
                        "Unable to get lock for reading filename " + fileName + " with baseDirectory " + baseDirectory +
                        ".", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");

                return result;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                if (logger != null)
                    _logger.LogErrorWithSource(ex,
                        "Exception while reading filename: " + fileName + " with baseDirectory: " + baseDirectory,
                        "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
                return result;
            }
        }

        public async Task<List<PersistentDataWrapper<T>>> ReadLike<T>(
            string pattern,
            string directory = null,
            bool useCache = true)
            where T : IPersistentData
        {
            var result = new List<PersistentDataWrapper<T>>();
            try
            {
                directory = directory ?? DataCachePath;
                if (Directory.Exists(directory))
                {
                    if (await _lock.WaitAsync(2000))
                        try
                        {
                            var strArray = Directory.GetFiles(directory, "*" + pattern + "*");
                            for (var index = 0; index < strArray.Length; ++index)
                            {
                                var path = strArray[index];
                                var persistentDataWrapperList = result;
                                persistentDataWrapperList.Add(await InnerRead<T>(Path.GetFileName(path), useCache,
                                    Path.GetDirectoryName(path), false));
                                persistentDataWrapperList = null;
                            }

                            strArray = null;
                        }
                        finally
                        {
                            _lock.Release();
                        }
                    else
                        _logger.LogErrorWithSource(
                            "Unable to get lock for reading with pattern " + pattern + " in directory " + directory +
                            ".", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
                }

                return result;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                if (logger != null)
                    _logger.LogErrorWithSource(ex,
                        "unhandled exception occurred calling ReadLike(" + pattern + " in directory: " + directory +
                        ")", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
                return result;
            }
        }

        public async Task<List<PersistentDataWrapper<T>>> ReadLike<T>(
            Regex pattern,
            string directory = null,
            bool useCache = true)
            where T : IPersistentData
        {
            var result = new List<PersistentDataWrapper<T>>();
            try
            {
                directory = directory ?? DataCachePath;
                if (Directory.Exists(directory))
                {
                    if (await _lock.WaitAsync(2000))
                        try
                        {
                            var strArray = Directory.GetFiles(directory, "*.*")
                                .Where(f => pattern.IsMatch(Path.GetFileName(f))).ToArray();
                            for (var index = 0; index < strArray.Length; ++index)
                            {
                                var path = strArray[index];
                                var persistentDataWrapperList = result;
                                persistentDataWrapperList.Add(await InnerRead<T>(Path.GetFileName(path), useCache,
                                    Path.GetDirectoryName(path), false));
                                persistentDataWrapperList = null;
                            }

                            strArray = null;
                        }
                        finally
                        {
                            _lock.Release();
                        }
                    else
                        _logger.LogErrorWithSource(
                            string.Format("Unable to get lock for reading with regex pattern {0} in directory {1}.",
                                pattern, directory),
                            "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
                }

                return result;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                if (logger != null)
                    _logger.LogErrorWithSource(ex,
                        string.Format("unhandled exception occurred calling ReadLike({0} in directory {1})", pattern,
                            directory), "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
                return result;
            }
        }

        public async Task<bool> Write<T>(T data, string fileName, string baseDirectory = null) where T : IPersistentData
        {
            try
            {
                var logger = _logger;
                if (logger != null)
                    _logger.LogInfoWithSource("Write(" + fileName + ")",
                        "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
                var json = JsonConvert.SerializeObject(new PersistentDataWrapper<T>
                {
                    Data = data,
                    Modified = DateTime.Now,
                    DataType = typeof(T).FullName
                }, (Formatting)1, new JsonSerializerSettings
                {
                    NullValueHandling = (NullValueHandling)1
                });
                var filePath = baseDirectory == null
                    ? GetDataFilePath(fileName)
                    : Path.Combine(baseDirectory, fileName);
                if (baseDirectory != null)
                    Directory.CreateDirectory(baseDirectory);
                if (await _lock.WaitAsync(2000))
                {
                    try
                    {
                        File.WriteAllText(filePath, json);
                        ClearCache(fileName);
                    }
                    finally
                    {
                        _lock.Release();
                    }

                    return true;
                }

                _logger.LogErrorWithSource(
                    "Unable to get lock for writing to fileName " + fileName + " with baseDiretory " + baseDirectory +
                    ".", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
                return false;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                if (logger != null)
                    _logger.LogErrorWithSource(ex,
                        "Exception while writing to filename: " + fileName + " with baseDirectory " + baseDirectory,
                        "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
                return false;
            }
        }

        public async Task<bool> Delete(string fileName, string baseDirectory = null)
        {
            try
            {
                var logger = _logger;
                if (logger != null)
                    _logger.LogInfoWithSource("Delete(" + fileName + ")",
                        "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
                var filePath = baseDirectory == null
                    ? GetDataFilePath(fileName)
                    : Path.Combine(baseDirectory, fileName);
                if (await _lock.WaitAsync(2000))
                    try
                    {
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                        ClearCache(fileName);
                        return true;
                    }
                    finally
                    {
                        _lock.Release();
                    }

                _logger.LogErrorWithSource(
                    "Unable to get lock for deleting to fileName " + fileName + " with baseDiretory " + baseDirectory +
                    ".", "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
                return false;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                if (logger != null)
                    _logger.LogErrorWithSource(ex,
                        "Exception while deleting filename " + fileName + " with baseDirectory " + baseDirectory,
                        "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
                return false;
            }
        }

        public async Task<bool> DeleteLike(string pattern, string directory = null)
        {
            try
            {
                var logger = _logger;
                if (logger != null)
                    _logger.LogInfoWithSource("DeleteLike(" + pattern + ")",
                        "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
                directory = directory ?? DataCachePath;
                if (await _lock.WaitAsync(2000))
                    try
                    {
                        if (Directory.Exists(directory))
                            foreach (var file in Directory.GetFiles(directory, "*" + pattern + "*"))
                                File.Delete(file);
                        return true;
                    }
                    finally
                    {
                        _lock.Release();
                    }

                _logger.LogErrorWithSource(
                    "Unable to get lock for deleting files with pattern " + pattern + " in diretory " + directory + ".",
                    "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
                return false;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                if (logger != null)
                    _logger.LogErrorWithSource(ex,
                        "Exception while deleting with pattern: " + pattern + " and directory: " + directory,
                        "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
                return false;
            }
        }

        private async Task<PersistentDataWrapper<T>> InnerRead<T>(
            string fileName,
            bool useCache = false,
            string baseDirectory = null,
            bool log = true)
            where T : IPersistentData
        {
            var result = new PersistentDataWrapper<T>();
            try
            {
                if (log)
                {
                    var logger = _logger;
                    if (logger != null)
                        _logger.LogInfoWithSource(string.Format("Read({0},useCache={1})", fileName, useCache),
                            "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
                }

                object obj;
                if (useCache && _cache.TryGetValue(fileName, out obj) && obj != null)
                {
                    if (log)
                    {
                        var logger = _logger;
                        if (logger != null)
                            _logger.LogInformation("Found cached value");
                    }

                    return obj as PersistentDataWrapper<T>;
                }

                var path = baseDirectory == null ? GetDataFilePath(fileName) : Path.Combine(baseDirectory, fileName);
                if (File.Exists(path))
                    using (var file = File.Open(path, (FileMode)3, (FileAccess)1, (FileShare)3))
                    {
                        using (var reader = new StreamReader(file))
                        {
                            result = JsonConvert.DeserializeObject<PersistentDataWrapper<T>>(
                                await reader.ReadToEndAsync());
                        }
                    }

                if (useCache)
                    _cache[fileName] = result;
                return result;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                if (logger != null)
                    _logger.LogErrorWithSource(ex,
                        "Exception while reading filename: " + fileName + " with baseDirectory: " + baseDirectory,
                        "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
                return result;
            }
        }

        private void ClearCache(string fileName)
        {
            if (!_cache.ContainsKey(fileName))
                return;
            var logger = _logger;
            if (logger != null)
                _logger.LogInfoWithSource("Clearing cached value " + fileName,
                    "/sln/src/UpdateClientService.API/Services/PersistentDataCacheService.cs");
            _cache.TryRemove(fileName, out var _);
        }

        private string GetDataFilePath(string fileName)
        {
            return Path.Combine(DataCachePath, Path.ChangeExtension(fileName, "json"));
        }
    }
}