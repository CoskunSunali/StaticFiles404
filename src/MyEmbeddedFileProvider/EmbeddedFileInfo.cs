using Microsoft.AspNet.FileProviders;
using System;
using System.IO;
using System.Reflection;

namespace MyEmbeddedFileProvider
{
    public class EmbeddedFileInfo : IFileInfo
    {
        private readonly Assembly _assembly;
        private readonly string _resourcePath;
        private long? _length;

        public string Name { get; }
        public DateTimeOffset LastModified { get; }
        public string PhysicalPath => null; // Not directly accessible
        public bool IsDirectory => false;
        public bool Exists => true;
        public long Length
        {
            get
            {
                if (!_length.HasValue)
                {
                    using (Stream stream = _assembly.GetManifestResourceStream(_resourcePath))
                    {
                        _length = stream.Length;
                    }
                }

                return _length.Value;
            }
        }

        public EmbeddedFileInfo(Assembly assembly, string resourcePath, string name)
        {
            _assembly = assembly;
            _resourcePath = resourcePath;

            Name = name;
            LastModified = DateTimeOffset.MaxValue; // Not even necessary
        }

        public Stream CreateReadStream()
        {
            Stream stream = _assembly.GetManifestResourceStream(_resourcePath);

            if (!_length.HasValue)
            {
                _length = stream.Length;
            }

            return stream;
        }
    }
}
