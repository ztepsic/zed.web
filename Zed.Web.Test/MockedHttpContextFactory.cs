using System.Web;
using Moq;

namespace Zed.Web.Test {
    /// <summary>
    /// Factory class that creates mocked http context.
    /// </summary>
    public static class MockedHttpContextFactory {

        #region Methods

        /// <summary>
        /// Creates http context based on <see cref="Mock"/>object.
        /// </summary>
        /// <param name="targetUrl">Target/request URL</param>
        /// <param name="httpMethod">Http method</param>
        /// <returns>Http context mocked http context</returns>
        public static HttpContextBase CreateHttpContext(string targetUrl = null, string httpMethod = "GET") {
            // create the mock context, using the request and response
            var mockedHttpContextBuilder = new MockedHttpContextBuilder();

            // setup the mock request
            mockedHttpContextBuilder.RequestMock.Setup(m => m.AppRelativeCurrentExecutionFilePath).Returns(targetUrl);
            mockedHttpContextBuilder.RequestMock.Setup(m => m.HttpMethod).Returns(httpMethod);

            // setup the mock response
            mockedHttpContextBuilder.ResponseMock.Setup(m => m.ApplyAppPathModifier(It.IsAny<string>()))
                .Returns<string>(s => s);

            // return the mocked context
            return mockedHttpContextBuilder.GetResult();
        }

        #endregion

    }
}
