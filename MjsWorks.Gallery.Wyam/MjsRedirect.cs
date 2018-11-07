using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;
using Wyam.Common.Util;

namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// Handles redirected content by creating pages with meta refresh tags or other redirect files.
    /// </summary>
    /// <remarks>
    /// <para>When content moves you need some way to redirect from the old location to the new location.
    /// This is especially true when moving content from one publishing system to another that might
    /// have different conventions for things like paths.</para>
    /// <para>This module lets you manage redirected content
    /// by generating special pages that contain a "meta refresh tag". This tag tells client browsers
    /// that the content has moved and to redirect to the new location. Google and other search engines
    /// also understand meta refresh tags and will adjust their search indexes accordingly.</para>
    /// <para>Alternatively (or additionally), you can also create host-specific redirect files to
    /// control redirection on the server.</para>
    /// <para>By default, this module will read the paths that need to be redirected from the
    /// <c>RedirectFrom</c> metadata key. One or more paths can be specified in this metadata and
    /// corresponding redirect files will be created for each.</para>
    /// <para>This module outputs any meta refresh pages as well as any additional redirect files
    /// you specify. It does not output the original input files.</para>
    /// </remarks>
    /// <metadata cref="Keys.RedirectFrom" usage="Input" />
    /// <metadata cref="Keys.RelativeFilePath" usage="Output" />
    /// <metadata cref="Keys.WritePath" usage="Output" />
    /// <category>Content</category>
    public class MjsRedirect : IModule
    {
        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            List<IDocument> outputs = inputs
                .AsParallel()
                .SelectMany(context, input =>
                {
                    IReadOnlyList<IDocument> pathMappings = input.List<IDocument>(Keys.RedirectFrom);
                    if (pathMappings != null)
                    {
                        List<IDocument> metaRefreshDocuments = new List<IDocument>();
                        foreach (var pathMapping in pathMappings.Where(x => x != null))
                        {
                            var fromPath = pathMapping.FilePath("FromPath");
                            var toPath = pathMapping.FilePath("ToPath");
                            // Make sure it's a relative path
                            if (!fromPath.IsRelative)
                            {
                                Trace.Warning($"The redirect path must be relative for document {toPath}: {fromPath}");
                                continue;
                            }

                            // Record the redirect for additional processing
                            string url = toPath.FullPath;
                            Console.WriteLine($"path: {toPath}, url: {url}");

                            // Meta refresh documents
                            FilePath outputPath = fromPath;
                            if (!string.Equals(outputPath.Extension, ".html", StringComparison.OrdinalIgnoreCase))
                            {
                                outputPath = outputPath.AppendExtension(".html");
                            }

                            metaRefreshDocuments.Add(
                                context.GetDocument(
                                    context.GetContentStream($@"
<!doctype html>
<html>    
<head>      
<title>Redirected</title>      
<meta http-equiv=""refresh"" content=""0;url='{url}'"" />    
</head>    
<body> 
<p>This page has moved to a <a href=""{url}"">{url}</a></p> 
</body>  
</html>"),
                                    new MetadataItems
                                    {
                                        { Keys.RelativeFilePath, outputPath },
                                        { Keys.WritePath, outputPath }
                                    }));
                        }
                        return (IEnumerable<IDocument>)metaRefreshDocuments;
                    }
                    return new IDocument[] { };
                })
                .ToList();  // Need to materialize the parallel operation before creating the additional outputs

            return outputs;
        }
    }
}