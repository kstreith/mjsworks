using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;

namespace MjsWorks.Gallery.Wyam
{
    public class AddIndividualPaintingMetadataToGallery : IModule
    {
        private IEnumerable<IModule> _paintingDocModules;

        public AddIndividualPaintingMetadataToGallery()
        {
        }
        
        public AddIndividualPaintingMetadataToGallery WithPaintingDocuments(IEnumerable<IModule> paintingDocModules)
        {
            _paintingDocModules = paintingDocModules;
            return this;
        }

        public AddIndividualPaintingMetadataToGallery WithPaintingDocuments(params IModule[] paintingDocModules)
        {
            return WithPaintingDocuments((IEnumerable<IModule>)paintingDocModules);
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var paintingDocs = context.Execute(_paintingDocModules, new List<IDocument>());
            var paintingDocIndexByImage = paintingDocs.ToDictionary(x => x.String("File"));
            return inputs.Select(context, doc =>
            {
                var galleryImages = doc.Get<IDocument[]>("Images");
                List<IDocument> outputGalleryImageDocs = new List<IDocument>();
                foreach (var galleryImage in galleryImages)
                {
                    var galleryImageFile = galleryImage.String("Image");
                    System.Console.WriteLine($"Looking up {galleryImageFile}");
                    var paintingDocDetails = paintingDocIndexByImage[galleryImageFile];
                    //System.Console.WriteLine($"paintingDocDetails {galleryImageFile}"); // - {JsonConvert.SerializeObject(paintingDocDetails.Keys.ToList())}");
                    var price = paintingDocDetails.Get<double>("Price");
                    //System.Console.WriteLine($"price {price}");
                    //System.Console.WriteLine($"originalPrice {paintingDocDetails.Get<double?>("OriginalPrice")}");
                    var newGalleryImageDoc = context.GetDocument(galleryImage, new Dictionary<string, object>
                    {
                        ["Title"] = paintingDocDetails.String("Title"),
                        ["Size"] = paintingDocDetails.String("Size"),
                        ["Sold"] = paintingDocDetails.Bool("Sold"),
                        ["ForSale"] = paintingDocDetails.Bool("ForSale"),
                        ["SourceFileBase"] = paintingDocDetails.String("SourceFileBase"),
                        ["Price"] = price,
                        ["OriginalPrice"] = paintingDocDetails.Get<double?>("OriginalPrice")
                    });
                    outputGalleryImageDocs.Add(newGalleryImageDoc);
                }
                return context.GetDocument(doc, new Dictionary<string, object>
                {
                    ["Images"] = outputGalleryImageDocs.ToArray()
                });
            });
        }
    }
}
