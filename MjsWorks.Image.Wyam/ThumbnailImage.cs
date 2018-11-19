using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using System;
using System.Collections.Generic;
using System.IO;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;

namespace MjsWorks.Image.Wyam
{
    public class ThumbnailImage : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Select(context, doc =>
            {
                var relativePath = doc.FilePath(Keys.RelativeFilePath);
                Console.WriteLine($"reading {relativePath.FullPath}");
                var destinationPath = context.FileSystem.GetOutputPath(relativePath);
                destinationPath = destinationPath.InsertSuffix("-thumbnail").ChangeExtension("jpg");
                Console.WriteLine($"writing {Keys.WritePath} - {destinationPath.FullPath}");
                using (var stream = doc.GetStream())
                {
                    using (var image = SixLabors.ImageSharp.Image.Load(stream, out IImageFormat imageFormat))
                    {
                        var newWidth = 0;
                        var newHeight = 0;
                        if (image.Width > image.Height)
                        {
                            newWidth = 240;
                        }
                        else
                        {
                            newHeight = 240;
                        }
                        image.Mutate(x => x.Resize(newWidth, newHeight));
                        var outputStream = new MemoryStream();
                        image.Save(outputStream, imageFormat);
                        outputStream.Seek(0, SeekOrigin.Begin);                    
                        return context.GetDocument(doc, outputStream, new Dictionary<string, object>
                        {
                            [Keys.WritePath] = destinationPath
                        });
                    }
                }
            });
        }
    }
}
