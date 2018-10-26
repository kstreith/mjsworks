using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;
using Xunit;
using FluentAssertions;
using System;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace MjsWorks.Gallery.Wyam.Test
{
    public class FakeGalleriesModule : IModule
    {
        public List<IDocument> Documents { get; set; } = new List<IDocument>();
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return Documents;
        }
    }
    public class PaintingRelationshipToGalleriesTest
    {
        [Fact]
        public void Execute_Paintings_NoneInGallery_ParentGalleryJsonIsEmpty()
        {
            //Arrange
            var painting = new PaintingRelationshipToGalleries();
            var testContext = new TestExecutionContext();
            testContext.Settings["Host"] = "localhost";
            List<IDocument> testPaintings = GetTestPaintings();
            var fakeGalleries = new FakeGalleriesModule();
            painting.WithGalleryDocuments(fakeGalleries);

            //Act
            var outputDoc = painting.Execute(testPaintings, testContext);

            //Assert
            outputDoc.Should().NotBeNull();
            var outputDocLookup = ToParentGalleryDict(outputDoc);
            outputDocLookup.Should().ContainKey("flying-in-air").WhichValue.Should().HaveCount(0);
            outputDocLookup.Should().ContainKey("swimming-in-water").WhichValue.Should().HaveCount(0);
            outputDocLookup.Should().ContainKey("crawling-on-ground").WhichValue.Should().HaveCount(0);
        }

        private static Dictionary<string, Dictionary<string, GalleryPosition>> ToParentGalleryDict(IEnumerable<IDocument> outputDoc)
        {
            return outputDoc.ToDictionary(x => x.String("SourceFileBase"), y => JsonConvert.DeserializeObject<Dictionary<string, GalleryPosition>>(y.String("ParentGalleriesJson")));
        }

        private IDocument CreateGallery(string galleryName, params string[] images)
        {
            var galleryMetadata = new Dictionary<string, object>
            {
                ["SourceFileBase"] = galleryName,
            };
            var imageArray = images.Select(imageName => new TestDocument(new Dictionary<string, object>
            {
                ["image"] = imageName
            })).ToArray<IDocument>();
            galleryMetadata["images"] = imageArray;
            return new TestDocument(galleryMetadata);
        }

        [Fact]
        public void Execute_Paintings_OnlyOneImageInOneGallery_ParentGalleryJsonIsCorrect()
        {
            //Arrange
            var painting = new PaintingRelationshipToGalleries();
            var testContext = new TestExecutionContext();
            testContext.Settings["Host"] = "localhost";
            List<IDocument> testPaintings = GetTestPaintings();
            var fakeGalleries = new FakeGalleriesModule();
            fakeGalleries.Documents = new List<IDocument> { CreateGallery("gallery1", "/assets/painting/swimming.JPG") };
            painting.WithGalleryDocuments(fakeGalleries);

            //Act
            var outputDoc = painting.Execute(testPaintings, testContext);

            //Assert
            outputDoc.Should().NotBeNull();
            var outputDocLookup = ToParentGalleryDict(outputDoc);
            outputDocLookup.Should().ContainKey("flying-in-air").WhichValue.Should().HaveCount(0);
            outputDocLookup.Should().ContainKey("swimming-in-water").WhichValue.Should().ContainKey("gallery1")
                .WhichValue.Should().BeEquivalentTo(new GalleryPosition {
                    PreviousImage = "swimming-in-water",
                    NextImage = "swimming-in-water" });
            outputDocLookup.Should().ContainKey("crawling-on-ground").WhichValue.Should().HaveCount(0);
        }

        [Fact]
        public void Execute_Paintings_MultipleImagesInOneGallery_ParentGalleryJsonIsCorrect()
        {
            //Arrange
            var painting = new PaintingRelationshipToGalleries();
            var testContext = new TestExecutionContext();
            testContext.Settings["Host"] = "localhost";
            List<IDocument> testPaintings = GetTestPaintings();
            var fakeGalleries = new FakeGalleriesModule();
            fakeGalleries.Documents = new List<IDocument> { CreateGallery("gallery1", "/assets/painting/swimming.JPG", "/assets/painting/flying.JPG", "/assets/painting/crawling.JPG") };
            painting.WithGalleryDocuments(fakeGalleries);

            //Act
            var outputDoc = painting.Execute(testPaintings, testContext);

            //Assert
            outputDoc.Should().NotBeNull();
            var outputDocLookup = ToParentGalleryDict(outputDoc);
            outputDocLookup.Should().ContainKey("flying-in-air").WhichValue.Should().ContainKey("gallery1")
                .WhichValue.Should().BeEquivalentTo(new GalleryPosition
                {
                    PreviousImage = "swimming-in-water",
                    NextImage = "crawling-on-ground"
                });
            outputDocLookup.Should().ContainKey("swimming-in-water").WhichValue.Should().ContainKey("gallery1")
                .WhichValue.Should().BeEquivalentTo(new GalleryPosition
                {
                    PreviousImage = "crawling-on-ground",
                    NextImage = "flying-in-air"
                });
            outputDocLookup.Should().ContainKey("crawling-on-ground").WhichValue.Should().ContainKey("gallery1")
                .WhichValue.Should().BeEquivalentTo(new GalleryPosition
                {
                    PreviousImage = "flying-in-air",
                    NextImage = "swimming-in-water"
                });
        }

        [Fact]
        public void Execute_Paintings_OnlyOneImageInMultipleGalleries_ParentGalleryJsonIsCorrect()
        {
            //Arrange
            var painting = new PaintingRelationshipToGalleries();
            var testContext = new TestExecutionContext();
            testContext.Settings["Host"] = "localhost";
            List<IDocument> testPaintings = GetTestPaintings();
            var fakeGalleries = new FakeGalleriesModule();
            fakeGalleries.Documents = new List<IDocument> {
                CreateGallery("gallery1", "/assets/painting/swimming.JPG"),
                CreateGallery("gallery2", "/assets/painting/swimming.JPG")
            };
            painting.WithGalleryDocuments(fakeGalleries);

            //Act
            var outputDoc = painting.Execute(testPaintings, testContext);

            //Assert
            outputDoc.Should().NotBeNull();
            var outputDocLookup = ToParentGalleryDict(outputDoc);
            outputDocLookup.Should().ContainKey("flying-in-air").WhichValue.Should().HaveCount(0);
            outputDocLookup.Should().ContainKey("swimming-in-water").WhichValue.Should().ContainKey("gallery1")
                .WhichValue.Should().BeEquivalentTo(new GalleryPosition
                {
                    PreviousImage = "swimming-in-water",
                    NextImage = "swimming-in-water"
                });
            outputDocLookup.Should().ContainKey("swimming-in-water").WhichValue.Should().ContainKey("gallery2")
                .WhichValue.Should().BeEquivalentTo(new GalleryPosition
                {
                    PreviousImage = "swimming-in-water",
                    NextImage = "swimming-in-water"
                });
            outputDocLookup.Should().ContainKey("crawling-on-ground").WhichValue.Should().HaveCount(0);
        }

        [Fact]
        public void Execute_Paintings_MultipleImagesWithOneNotFoundImageInGallery_ThrowsException()
        {
            //Arrange
            var painting = new PaintingRelationshipToGalleries();
            var testContext = new TestExecutionContext();
            testContext.Settings["Host"] = "localhost";
            List<IDocument> testPaintings = GetTestPaintings();
            var fakeGalleries = new FakeGalleriesModule();
            fakeGalleries.Documents = new List<IDocument> { CreateGallery("gallery1", "/assets/painting/swimming.JPG", "/assets/painting/not-found.JPG") };
            painting.WithGalleryDocuments(fakeGalleries);

            //Act
            void Act() => painting.Execute(testPaintings, testContext).ToList();

            //Assert
            Assert.Throws<KeyNotFoundException>(new Action(Act));
        }

        [Fact]
        public void Execute_Paintings_OnlyOneNotFoundImageInOneGallery_ParentGalleryJsonIsCorrect()
        {
            //Arrange
            var painting = new PaintingRelationshipToGalleries();
            var testContext = new TestExecutionContext();
            testContext.Settings["Host"] = "localhost";
            List<IDocument> testPaintings = GetTestPaintings();
            var fakeGalleries = new FakeGalleriesModule();
            fakeGalleries.Documents = new List<IDocument> { CreateGallery("gallery1", "/assets/painting/not-found.JPG") };
            painting.WithGalleryDocuments(fakeGalleries);

            //Act
            var outputDoc = painting.Execute(testPaintings, testContext);

            //Assert
            outputDoc.Should().NotBeNull();
            var outputDocLookup = ToParentGalleryDict(outputDoc);
            outputDocLookup.Should().ContainKey("flying-in-air").WhichValue.Should().HaveCount(0);
            outputDocLookup.Should().ContainKey("swimming-in-water").WhichValue.Should().HaveCount(0);
            outputDocLookup.Should().ContainKey("crawling-on-ground").WhichValue.Should().HaveCount(0);
        }

        private static List<IDocument> GetTestPaintings()
        {
            var testFlyingPainting = new TestDocument(new Dictionary<string, object>
            {
                ["SourceFileBase"] = "flying-in-air",
                ["File"] = "/assets/painting/flying.JPG"
            });
            var testSwimmingPainting = new TestDocument(new Dictionary<string, object>
            {
                ["SourceFileBase"] = "swimming-in-water",
                ["File"] = "/assets/painting/swimming.JPG"
            });
            var testCrawlingPainting = new TestDocument(new Dictionary<string, object>
            {
                ["SourceFileBase"] = "crawling-on-ground",
                ["File"] = "/assets/painting/crawling.JPG"
            });
            var testPaintings = new List<IDocument> { testFlyingPainting, testSwimmingPainting, testCrawlingPainting };
            return testPaintings;
        }
    }
}
