using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System.Collections.Generic;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;

namespace MjsWorks.Image.Wyam
{
    public class ImageInfo : IModule
    {
        private string _imagePathKey = "File";
        private string _imageMetadataPrefix = "Image";
        public ImageInfo()
        {
        }

        public ImageInfo FromMetadataKey(string key)
        {
            _imagePathKey = key;
            return this;
        }

        public ImageInfo OutputMetadataWithPrefix(string prefix)
        {
            _imageMetadataPrefix = prefix;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Select(context, doc =>
            {
                var imageRelativeFilePath = doc.String(_imagePathKey);
                if (imageRelativeFilePath.StartsWith("/"))
                {
                    imageRelativeFilePath = imageRelativeFilePath.Substring(1);
                }
                var filePath = new FilePath(imageRelativeFilePath);
                var sourceDir = doc.DirectoryPath(Keys.SourceFileRoot);
                var imagePath = sourceDir.CombineFile(imageRelativeFilePath);
                using (var image = SixLabors.ImageSharp.Image.Load(imagePath.FullPath, out IImageFormat imageFormat))
                {
                    return context.GetDocument(doc, new Dictionary<string, object>
                    {
                        [$"{_imageMetadataPrefix}Width"] = image.Width,
                        [$"{_imageMetadataPrefix}Height"] = image.Height,
                        [$"{_imageMetadataPrefix}MimeType"] = imageFormat.DefaultMimeType
                    });
                }
            });
        }
    }
}
