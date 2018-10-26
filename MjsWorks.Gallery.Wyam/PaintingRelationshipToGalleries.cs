using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;

namespace MjsWorks.Gallery.Wyam
{
    public class GalleryPosition {
        public string NextImage { get; set; }
        public string PreviousImage { get; set; }
    }
    public class PaintingRelationshipToGalleries : IModule
    {
        private string _priceKey = "Price";
        private string _outputOriginalPriceKey = "OriginalPrice";
        private double? _discount = null;
        private IEnumerable<IModule> _galleryModules;

        public PaintingRelationshipToGalleries()
        {
        }

        public PaintingRelationshipToGalleries WithDiscount(double discount)
        {
            _discount = discount;
            return this;
        }

        public PaintingRelationshipToGalleries FromMetadataKey(string priceKey)
        {
            _priceKey = priceKey;
            return this;
        }

        public PaintingRelationshipToGalleries OutputMetadataKey(string originalPriceKey)
        {
            _outputOriginalPriceKey = originalPriceKey;
            return this;
        }

        
        public PaintingRelationshipToGalleries WithGalleryDocuments(IEnumerable<IModule> galleryDocs)
        {
            _galleryModules = galleryDocs;
            return this;
        }

        public PaintingRelationshipToGalleries WithGalleryDocuments(params IModule[] modules)
        {
            return WithGalleryDocuments((IEnumerable<IModule>)modules);
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var galleryDocs = context.Execute(_galleryModules, new List<IDocument>());
            var galleryIndex = galleryDocs.ToDictionary(x => x.String("SourceFileBase"));
            var imageWithinGalleries = galleryDocs.SelectMany(x => x.Get<IDocument[]>("images").Select(y => new { gallery = x.String("SourceFileBase"), image = y.String("image") })).ToLookup(z => z.image, z => z.gallery);
            System.Console.WriteLine($"imageWithGalleries: {JsonConvert.SerializeObject(imageWithinGalleries, Formatting.Indented)}");
            System.Console.WriteLine($"galleryDocs: {JsonConvert.SerializeObject(galleryDocs.Select(x => x.Get("images").GetType().Name))}");
            var paintingByImageFile = inputs.ToDictionary(x => x.String("File"), x => x.String("SourceFileBase"));
            return inputs.Select(context, doc =>
            {
                var file = doc.String("File");
                var parentGalleryNames = imageWithinGalleries[file];
                System.Console.WriteLine($"parentGalleryNames for {file}: {JsonConvert.SerializeObject(parentGalleryNames)}");
                var parentGalleryNavigations = new Dictionary<string, GalleryPosition>();
                foreach (var parentGalleryName in parentGalleryNames)
                {
                    var galleryImages = galleryIndex[parentGalleryName].Get<IDocument[]>("images");
                    var foundIndex = -1;
                    for (var i = 0; i < galleryImages.Length; ++i)
                    {
                        if (galleryImages[i].String("image") == file)
                        {
                            foundIndex = i;
                        }
                    }
                    if (foundIndex != -1)
                    {
                        string prevImageFile = null;
                        string nextImageFile = null;
                        if (foundIndex == 0)
                        {
                            prevImageFile = galleryImages[galleryImages.Length - 1].String("image");
                        }
                        else
                        {
                            prevImageFile = galleryImages[foundIndex - 1].String("image");
                        }
                        if (foundIndex == galleryImages.Length - 1)
                        {
                            nextImageFile = galleryImages[0].String("image");
                        }
                        else
                        {
                            nextImageFile = galleryImages[foundIndex + 1].String("image");
                        }
                        var prevImagePage = paintingByImageFile[prevImageFile];
                        var nextImagePage = paintingByImageFile[nextImageFile];
                        parentGalleryNavigations[parentGalleryName] = new GalleryPosition { PreviousImage = prevImagePage, NextImage = nextImagePage };
                        System.Console.WriteLine($"parentGallery: {parentGalleryName} PrevImage: {prevImageFile} {prevImagePage}, NextImage: {nextImageFile} {nextImagePage}");
                    }
                }
                /*var parentGalleries = new Dictionary<string, GalleryPosition>
                {
                    ["butterflies"] = new GalleryPosition { PreviousImage = "foo", NextImage = "bar" },
                    ["my-amazing-gallery"] = new GalleryPosition { PreviousImage = "a", NextImage = "b" }
                };*/
                var parentGalleriesJson = JsonConvert.SerializeObject(parentGalleryNavigations);
                return context.GetDocument(doc, new Dictionary<string, object>
                {
                    ["ParentGalleriesJson"] = parentGalleriesJson
                });
            });
        }
    }
}
