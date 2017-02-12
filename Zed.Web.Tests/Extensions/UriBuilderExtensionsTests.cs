using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;
using Zed.Web.Extensions;

namespace Zed.Web.Tests.Extensions {
    [TestFixture]
    public class UriBuilderExtensionsTests {

        [Test]
        public void BuildQueryString_NameValueCollection_QueryString() {
            // Arrange
            const string baseUri = "http://example.com";
            const string path = "test";

            var uriBuilder = new UriBuilder(baseUri) {
                Path = path
            };


            // Act
            uriBuilder.BuildQueryString(
                new NameValueCollection() {
                    ["foo"] = "bar" ,
                    ["baz"] = "boom",
                    ["cow"] = "milk",
                    ["php"] = "hypertext processor"
                }
            );

            // Assert
            var uri = uriBuilder.Uri;
            Assert.AreEqual("?foo=bar&baz=boom&cow=milk&php=hypertext+processor", uri.Query);
            Assert.AreEqual($"{baseUri}/{path}?foo=bar&baz=boom&cow=milk&php=hypertext+processor", uri.AbsoluteUri);
        }


        [Test]
        public void BuildQueryString_AnonymousTypeObject_QueryString() {
            // Arrange
            const string baseUri = "http://example.com";
            const string path = "test";

            var uriBuilder = new UriBuilder(baseUri) {
                Path = path
            };


            // Act
            uriBuilder.BuildQueryString(
                new  {
                    foo = "bar",
                    baz = "boom",
                    cow = "milk",
                    php = "hypertext processor"
                }
            );

            // Assert
            var uri = uriBuilder.Uri;
            Assert.AreEqual("?foo=bar&baz=boom&cow=milk&php=hypertext+processor", uri.Query);
            Assert.AreEqual($"{baseUri}/{path}?foo=bar&baz=boom&cow=milk&php=hypertext+processor", uri.AbsoluteUri);
        }

        [Test]
        public void RemoveQueryParams_QueryParamsToRemove_QueryStringWithoutGivenQueryParams() {
            // Arrange
            const string baseUri = "http://example.com";
            const string path = "test";

            var uriBuilder = new UriBuilder(baseUri) {
                Path = path
            };

            uriBuilder.BuildQueryString(
                new {
                    foo = "bar",
                    baz = "boom",
                    cow = "milk",
                    php = "hypertext processor"
                }
            );

            // Act
            uriBuilder.RemoveQueryParams("baz", "php");

            // Assert
            var uri = uriBuilder.Uri;
            Assert.AreEqual("?foo=bar&cow=milk", uri.Query);
            Assert.AreEqual($"{baseUri}/{path}?foo=bar&cow=milk", uri.AbsoluteUri);

        }

        [Test]
        public void RemoveQueryParams_QueryParamsToRemoveIEnumerable_QueryStringWithoutGivenQueryParams() {
            // Arrange
            const string baseUri = "http://example.com";
            const string path = "test";

            var uriBuilder = new UriBuilder(baseUri) {
                Path = path
            };

            uriBuilder.BuildQueryString(
                new {
                    foo = "bar",
                    baz = "boom",
                    cow = "milk",
                    php = "hypertext processor"
                }
            );

            // Act
            uriBuilder.RemoveQueryParams(new List<string>() { "baz", "php"});

            // Assert
            var uri = uriBuilder.Uri;
            Assert.AreEqual("?foo=bar&cow=milk", uri.Query);
            Assert.AreEqual($"{baseUri}/{path}?foo=bar&cow=milk", uri.AbsoluteUri);

        }

        [Test]
        public void RemoveQueryParam_QueryParamToRemove_QueryStringWithoutGivenQueryParam() {
            // Arrange
            const string baseUri = "http://example.com";
            const string path = "test";

            var uriBuilder = new UriBuilder(baseUri) {
                Path = path
            };

            uriBuilder.BuildQueryString(
                new {
                    foo = "bar",
                    baz = "boom",
                    cow = "milk",
                    php = "hypertext processor"
                }
            );

            // Act
            uriBuilder.RemoveQueryParam("baz");

            // Assert
            var uri = uriBuilder.Uri;
            Assert.AreEqual("?foo=bar&cow=milk&php=hypertext+processor", uri.Query);
            Assert.AreEqual($"{baseUri}/{path}?foo=bar&cow=milk&php=hypertext+processor", uri.AbsoluteUri);

        }

        [Test]
        public void Combine_UriSegments1_UriInstance() {
            // Arrange
            const string baseUri = "http://example.com";
            const string path = "test";

            // Act
            var uri = UriBuilderExtensions.Combine(
                baseUri,
                path,
                "/sub-path-with-slashes/"
                );

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual($"{baseUri}/{path}/sub-path-with-slashes", uri.ToString());
        }

        [Test]
        public void Combine_UriSegments2_UriInstance() {
            // Arrange
            const string baseUri = "http://example.com";
            const string path = "test";

            // Act
            var uri = UriBuilderExtensions.Combine(
                baseUri,
                path,
                "/sub-path-with-slashes/",
                "filename.ext?"
                );

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual($"{baseUri}/{path}/sub-path-with-slashes/filename.ext?", uri.ToString());
        }

        [Test]
        public void Combine_UriSegments3_UriInstance() {
            // Arrange
            const string baseUri = "http://example.com";
            const string path = "test";

            // Act
            var uri = UriBuilderExtensions.Combine(
                baseUri,
                path,
                "/sub-path-with-slashes/",
                "filename.ext?",
                "param1=1"
                );

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual($"{baseUri}/{path}/sub-path-with-slashes/filename.ext?param1=1", uri.ToString());
        }

        [Test]
        public void Combine_UriSegments4_UriInstance() {
            // Arrange
            const string baseUri = "http://example.com";
            const string path = "test";

            // Act
            var uri = UriBuilderExtensions.Combine(
                baseUri,
                path,
                "/sub-path-with-slashes/",
                "filename.ext?",
                "param1=1",
                "&param2=2"
                );

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual($"{baseUri}/{path}/sub-path-with-slashes/filename.ext?param1=1&param2=2", uri.ToString());
        }

        [Test]
        public void Combine_UriSegments41_UriInstance() {
            // Arrange
            const string baseUri = "http://example.com";
            const string path = "test";

            // Act
            var uri = UriBuilderExtensions.Combine(
                baseUri,
                path,
                "/sub-path-with-slashes/",
                "filename.ext?",
                "?param1=1",
                "&param2=2"
                );

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual($"{baseUri}/{path}/sub-path-with-slashes/filename.ext?param1=1&param2=2", uri.ToString());
        }


        [Test]
        public void Combine_UriSegments5_UriInstance() {
            // Arrange
            const string baseUri = "http://example.com";
            const string path = "test";

            // Act
            var uri = UriBuilderExtensions.Combine(
                baseUri,
                path,
                "/sub-path-with-slashes/",
                "filename.ext?",
                "param1=1",
                "&param2=2",
                "#fragment"
                );

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual($"{baseUri}/{path}/sub-path-with-slashes/filename.ext?param1=1&param2=2#fragment", uri.ToString());
        }


        [Test]
        public void PathCombine_PathSegments_UriBuilderInstanceWithPath() {
            // Arrange
            const string baseUri = "http://example.com";
            const string path = "test";

            // Act
            var uriBuilder = new UriBuilder(baseUri)
                .PathCombine(
                    path,
                    "/sub-path-with-slashes/",
                    "filename.ext");

            // Assert
            var uri = uriBuilder.Uri;

            Assert.IsNotNull(uri);
            Assert.AreEqual($"{baseUri}/{path}/sub-path-with-slashes/filename.ext", uri.ToString());
        }

        [Test]
        public void AppendPathSegment_PathSegment_UriBuilderInstanceWithPath() {
            // Arrange
            const string baseUri = "http://example.com";
            const string path = "test";

            // Act
            var uriBuilder = new UriBuilder(baseUri)
                .PathCombine(path);

            uriBuilder.AppendPathSegment("/sub-path-with-slashes/");
            uriBuilder.AppendPathSegment("filename.ext");


            // Assert
            var uri = uriBuilder.Uri;

            Assert.IsNotNull(uri);
            Assert.AreEqual($"{baseUri}/{path}/sub-path-with-slashes/filename.ext", uri.ToString());
        }


        [Test]
        public void AppendPathSegments_PathSegments_UriBuilderInstanceWithPath() {
            // Arrange
            const string baseUri = "http://example.com";
            const string path = "test";

            // Act
            var uriBuilder = new UriBuilder(baseUri)
                .PathCombine(path);

            uriBuilder.AppendPathSegments(
                "/sub-path-with-slashes/",
                "filename.ext");


            // Assert
            var uri = uriBuilder.Uri;

            Assert.IsNotNull(uri);
            Assert.AreEqual($"{baseUri}/{path}/sub-path-with-slashes/filename.ext", uri.ToString());
        }


    }

}
