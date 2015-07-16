using System;
using Microsoft.AspNet.FileProviders;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Microsoft.Framework.Caching;

namespace MyEmbeddedFileProvider
{
    public class EmbeddedFileProvider : IFileProvider
    {
        private readonly IFileProvider _defaultFileProvider;

        private readonly TimeSpan _cacheDuration = TimeSpan.FromDays(1);
        private readonly ConcurrentDictionary<string, ExpiringFileInfo> _cachedFiles =
            new ConcurrentDictionary<string, ExpiringFileInfo>(StringComparer.OrdinalIgnoreCase);

        public EmbeddedFileProvider(
            IFileProvider defaultFileProvider)
        {
            _defaultFileProvider = defaultFileProvider;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _defaultFileProvider.GetDirectoryContents(subpath);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            IFileInfo physicalFile = _defaultFileProvider.GetFileInfo(subpath);
            if (physicalFile != null && physicalFile.Exists)
            {
                return physicalFile;
            }

            ExpiringFileInfo file;
            if (_cachedFiles.TryGetValue(subpath, out file))
            {
                if (file.Expires > DateTime.UtcNow)
                {
                    return file.FileInfo;
                }
            }

            // check assemblies
            subpath = subpath.StartsWith("/") ? subpath.Substring(1) : subpath;

            string directoryName =
                Path.GetDirectoryName(subpath)
                    .Replace('\\', '.')
                    .Replace('/', '.')
                    .Replace('-', '_');

            string fileName = Path.GetFileName(subpath);

            DateTime expires = DateTime.UtcNow.Add(_cacheDuration);

            Assembly assembly = this.GetType().GetTypeInfo().Assembly;


            string baseNamespace = assembly.GetName().Name;
            string resourcePath =
                string.Format(
                    "{0}.{1}.{2}",
                    baseNamespace,
                    directoryName,
                    fileName);

            if (assembly.GetManifestResourceInfo(resourcePath) != null)
            {
                file = new ExpiringFileInfo(
                    assembly,
                    resourcePath,
                    fileName,
                    expires);
            }
            else
            {
                file = new ExpiringFileInfo(new NotFoundFileInfo(fileName), expires);
            }

            _cachedFiles[subpath] = file;

            return file.FileInfo;
        }

        public IExpirationTrigger Watch(string filter)
        {
            return _defaultFileProvider.Watch(filter);
        }
    }
}
