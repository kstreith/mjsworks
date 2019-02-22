using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;
using Xunit;
using FluentAssertions;
using System;
using Wyam.Core.Modules.Contents;

namespace MjsWorks.Blog.Wyam.Test
{
    public class FilterAndSortPostsTests
    {
        [Fact]
        public void Execute_FilterAndSortPosts_RemovesDocuments_With_PublishedDateInFuture()
        {
            //Arrange
            var filterModule = new FilterAndSortPosts();
            var testContext = new TestExecutionContext();
            List<IDocument> testPosts = GetTestPosts();

            //Act
            var outputDoc = filterModule.WithFilterDate(new DateTime(2011, 12, 3)).Execute(testPosts, testContext);

            //Assert
            outputDoc.Count().Should().Be(0);
        }

        [Fact]
        public void Execute_FilterAndSortPosts_RemovesDocuments_Without_PublishedDate()
        {
            //Arrange
            var filterModule = new FilterAndSortPosts();
            var testContext = new TestExecutionContext();
            List<IDocument> testPosts = GetTestPosts(true);

            //Act
            var outputDoc = filterModule.WithFilterDate(new DateTime(2011, 12, 3)).Execute(testPosts, testContext);

            //Assert
            outputDoc.Count().Should().Be(0);
        }

        [Fact]
        public void Execute_FilterAndSortPosts_KeepsDocuments_With_PublishedDatesBeforeToday()
        {
            //Arrange
            var filterModule = new FilterAndSortPosts();
            var testContext = new TestExecutionContext();
            List<IDocument> testPosts = GetTestPosts();

            //Act
            var outputDoc = filterModule.WithFilterDate(new DateTime(2018, 12, 16)).Execute(testPosts, testContext);

            //Assert
            outputDoc.Count().Should().Be(1);
        }

        [Fact]
        public void Execute_FilterAndSortPosts_KeepsDocuments_With_PublishedDatesOnToday()
        {
            //Arrange
            var filterModule = new FilterAndSortPosts();
            var testContext = new TestExecutionContext();
            List<IDocument> testPosts = GetTestPosts();

            //Act
            var outputDoc = filterModule.WithFilterDate(new DateTime(2018, 12, 17)).Execute(testPosts, testContext);

            //Assert
            outputDoc.Count().Should().Be(2);
        }

        [Fact]
        public void Execute_FilterAndSortPosts_Sorts_MostRecentPublishedPost_First()
        {
            //Arrange
            var filterModule = new FilterAndSortPosts();
            var testContext = new TestExecutionContext();
            List<IDocument> testPosts = GetTestPosts();

            //Act
            var outputDoc = filterModule.WithFilterDate(new DateTime(2018, 12, 17)).Execute(testPosts, testContext);

            //Assert
            var outputList = outputDoc.ToList();
            outputList.Count.Should().Be(2);
            outputList[0].Get<DateTime>("Published").Should().BeCloseTo(new DateTime(2018, 12, 17));
            outputList[1].Get<DateTime>("Published").Should().BeCloseTo(new DateTime(2018, 12, 15));
        }

        private static List<IDocument> GetTestPosts(bool includeDocWithoutPublished = false)
        {
            var testPost1 = new TestDocument(new Dictionary<string, object>
            {
                ["Published"] = new DateTime(2018, 12, 15),
            });
            var testPost2 = new TestDocument(new Dictionary<string, object>
            {
                ["Published"] = new DateTime(2018, 12, 17),
            });
            var testPosts = new List<IDocument> { testPost1, testPost2 };
            if (includeDocWithoutPublished)
            {
                var testPost3 = new TestDocument(new Dictionary<string, object>());
                testPosts.Add(testPost3);
            }
            return testPosts;
        }
    }
}
