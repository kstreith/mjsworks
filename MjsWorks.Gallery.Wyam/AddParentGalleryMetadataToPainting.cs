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
    public class AddParentGalleryMetadataToPainting : IModule
    {
        private IEnumerable<IModule> _galleryDocModules;

        public AddParentGalleryMetadataToPainting()
        {
        }

        
        public AddParentGalleryMetadataToPainting WithGalleryDocuments(IEnumerable<IModule> galleryDocModules)
        {
            _galleryDocModules = galleryDocModules;
            return this;
        }

        public AddParentGalleryMetadataToPainting WithGalleryDocuments(params IModule[] modules)
        {
            return WithGalleryDocuments((IEnumerable<IModule>)modules);
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var galleryDocs = context.Execute(_galleryDocModules, new List<IDocument>());
            var galleryIndex = galleryDocs.ToDictionary(x => x.String("SourceFileBase"));
            var imageWithinGalleries = galleryDocs.SelectMany(x => x.Get<IDocument[]>("Images").Select(y => new { gallery = x.String("SourceFileBase"), image = y.String("Image") })).ToLookup(z => z.image, z => z.gallery);
            //System.Console.WriteLine($"imageWithGalleries: {JsonConvert.SerializeObject(imageWithinGalleries, Formatting.Indented)}");
            //System.Console.WriteLine($"galleryDocs: {JsonConvert.SerializeObject(galleryDocs.Select(x => x.Get("images").GetType().Name))}");
            var paintingByImageFile = inputs.ToDictionary(x => x.String("File"), x => x.String("SourceFileBase"));
            return inputs.Select(context, doc =>
            {
                var file = doc.String("File");
                var parentGalleryNames = imageWithinGalleries[file];
                //System.Console.WriteLine($"parentGalleryNames for {file}: {JsonConvert.SerializeObject(parentGalleryNames)}");
                var parentGalleryNavigations = new Dictionary<string, GalleryPosition>();
                foreach (var parentGalleryName in parentGalleryNames)
                {
                    var galleryImages = galleryIndex[parentGalleryName].Get<IDocument[]>("Images");
                    var foundIndex = -1;
                    for (var i = 0; i < galleryImages.Length; ++i)
                    {
                        if (galleryImages[i].String("Image") == file)
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
                            prevImageFile = galleryImages[galleryImages.Length - 1].String("Image");
                        }
                        else
                        {
                            prevImageFile = galleryImages[foundIndex - 1].String("Image");
                        }
                        if (foundIndex == galleryImages.Length - 1)
                        {
                            nextImageFile = galleryImages[0].String("Image");
                        }
                        else
                        {
                            nextImageFile = galleryImages[foundIndex + 1].String("Image");
                        }
                        var prevImagePage = paintingByImageFile[prevImageFile];
                        var nextImagePage = paintingByImageFile[nextImageFile];
                        parentGalleryNavigations[parentGalleryName] = new GalleryPosition { PreviousImage = prevImagePage, NextImage = nextImagePage };
                        //System.Console.WriteLine($"parentGallery: {parentGalleryName} PrevImage: {prevImageFile} {prevImagePage}, NextImage: {nextImageFile} {nextImagePage}");
                    }
                }
                var salesImages = new List<string> {
                    doc.String("File")
                };
                var additionalImages = doc.Get<string[]>("AdditionalImages");
                if (additionalImages?.Any() == true) {
                    salesImages.AddRange(additionalImages);
                }
                var parentGalleriesJson = JsonConvert.SerializeObject(parentGalleryNavigations);
                var salesImagesJson = JsonConvert.SerializeObject(salesImages);
                return context.GetDocument(doc, new Dictionary<string, object>
                {
                    ["ParentGalleriesJson"] = parentGalleriesJson,
                    ["salesImagesJson"] = salesImagesJson
                });
            });
        }
    }
}
