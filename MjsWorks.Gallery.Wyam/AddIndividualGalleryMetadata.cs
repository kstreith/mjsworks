using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;

namespace MjsWorks.Gallery.Wyam
{
    public class AddIndividualGalleryMetadata : IModule
    {
        private IEnumerable<IModule> _galleryDocModules;

        public AddIndividualGalleryMetadata()
        {
        }
        
        public AddIndividualGalleryMetadata WithGalleryDocuments(IEnumerable<IModule> galleryDocModules)
        {
            _galleryDocModules = galleryDocModules;
            return this;
        }

        public AddIndividualGalleryMetadata WithGalleryDocuments(params IModule[] galleryDocModules)
        {
            return WithGalleryDocuments((IEnumerable<IModule>)galleryDocModules);
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var galleryDocs = context.Execute(_galleryDocModules, new List<IDocument>());
            var galleryDocIndexByName = galleryDocs.ToDictionary(x => x.String("SourceFileBase"));
            return inputs.Select(context, doc =>
            {
                var galleries = doc.Get<IDocument[]>("Galleries");
                List<IDocument> outputGalleryDetailDocs = new List<IDocument>();
                foreach (var gallery in galleries)
                {
                    var galleryName = gallery.String("Gallery");
                    System.Console.WriteLine($"GalleryName: {galleryName}");
                    var galleryDocDetails = galleryDocIndexByName[galleryName];
                    //System.Console.WriteLine($"paintingDocDetails {galleryImageFile}"); // - {JsonConvert.SerializeObject(paintingDocDetails.Keys.ToList())}");
                    //System.Console.WriteLine($"price {price}");
                    //System.Console.WriteLine($"originalPrice {paintingDocDetails.Get<double?>("OriginalPrice")}");
                    var newGalleryDetailDoc = context.GetDocument(gallery, new Dictionary<string, object>
                    {
                        ["Title"] = galleryDocDetails.String("Title"),
                        ["CoverImage"] = galleryDocDetails.String("CoverImage"),
                        ["Summary"] = galleryDocDetails.String("Summary")
                    });
                    outputGalleryDetailDocs.Add(newGalleryDetailDoc);
                }
                return context.GetDocument(doc, new Dictionary<string, object>
                {
                    ["Galleries"] = outputGalleryDetailDocs.ToArray()
                });
            });
        }
    }
}
