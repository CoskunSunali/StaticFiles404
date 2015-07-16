using Microsoft.AspNet.FileProviders;
using System;
using System.Reflection;

namespace MyEmbeddedFileProvider
{
    public class ExpiringFileInfo
    {
        public IFileInfo FileInfo { get; }
        public DateTime Expires { get; }

        public ExpiringFileInfo(Assembly assembly, string resourcePath, string name, DateTime expires)
            : this(new EmbeddedFileInfo(assembly, resourcePath, name), expires)
        {

        }

        public ExpiringFileInfo(IFileInfo fileInfo, DateTime expires)
        {
            FileInfo = fileInfo;
            Expires = expires;
        }

    }
}
