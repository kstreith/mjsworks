using System.Collections.Generic;
using System.IO;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;

namespace MjsWorks.Gallery.Wyam
{
    public class RenderGalleryAsIndexFileWithinFolder : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Select(context, doc =>
            {
                var currentRelativeFileDir = doc.DirectoryPath(Keys.RelativeFileDir);
                //var newFileDir = new DirectoryPath(currentRelativeFileDir + "/index");
                var currentRelativeFilePath = doc.FilePath(Keys.RelativeFilePath);
                var newFilePath = currentRelativeFileDir.Combine(doc.String("SourceFileBase")).CombineFile("index" + currentRelativeFilePath.Extension);
                //var newFilePath = new FilePath(currentRelativeFileDir + currentRelativeFilePath.FileNameWithoutExtension + "/index");
                System.Console.WriteLine($"Original Dir {currentRelativeFileDir}");
                System.Console.WriteLine($"Original Path {currentRelativeFilePath} to {newFilePath}");
                return context.GetDocument(doc, new Dictionary<string, object>
                {
                    [Keys.RelativeFilePath] = newFilePath
                });
            });
        }
    }
}
