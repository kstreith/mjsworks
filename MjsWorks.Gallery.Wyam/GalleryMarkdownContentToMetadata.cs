using System.Collections.Generic;
using System.IO;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;

namespace MjsWorks.Gallery.Wyam
{
    public class GalleryMarkdownContentToMetadata : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Select(context, doc =>
            {
                return context.GetDocument(doc, new Dictionary<string, object>
                {
                    ["FileContent"] = doc.Content
                });
            });
        }
    }
}
