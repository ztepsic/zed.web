using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using System.Net;
using Zed.Errors;
using Zed.Web.Extensions;

namespace Zed.Web.Tests.Extensions {
    /// <summary>
    /// Unit tests for <see cref="FluentResultExtensions"/>.
    /// </summary>
    public class FluentResultExtensionsTests {

        #region Fields

        private readonly Mock<ProblemDetailsFactory> mockFactory;
        private readonly DefaultHttpContext httpContext;

        /// <summary>Provides extra-error variants for mixed-error theory tests.</summary>
        public static TheoryData<IError> MixedErrorCombinations => new()
        {
            new Error("Additional context."),
            new ValidationError("Name", "Name is required.")
        };

        #endregion

        #region Constructors

        public FluentResultExtensionsTests() {
            mockFactory = new Mock<ProblemDetailsFactory>();

            mockFactory
                .Setup(f => f.CreateProblemDetails(
                    It.IsAny<HttpContext>(),
                    It.IsAny<int?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>()))
                .Returns<HttpContext, int?, string?, string?, string?, string?>(
                    (_, statusCode, _, _, detail, _) =>
                        new ProblemDetails { Status = statusCode, Detail = detail });

            mockFactory
                .Setup(f => f.CreateValidationProblemDetails(
                    It.IsAny<HttpContext>(),
                    It.IsAny<ModelStateDictionary>(),
                    It.IsAny<int?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>()))
                .Returns<HttpContext, ModelStateDictionary, int?, string?, string?, string?, string?>(
                    (_, _, statusCode, _, _, _, _) =>
                        new ValidationProblemDetails { Status = statusCode ?? StatusCodes.Status400BadRequest });

            httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/api/test";
            httpContext.Request.QueryString = new QueryString("?q=1");
        }

        #endregion

        #region Methods

        // ---- Argument guards (B3) ----

        [Fact]
        public void ToProblemDetailsResult_ThrowsArgumentNullException_When_Result_Is_Null() {
            // Arrange
            Result result = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                result.ToProblemDetailsResult(mockFactory.Object, httpContext));
        }

        [Fact]
        public void ToProblemDetailsResult_ThrowsArgumentNullException_When_ProblemDetailsFactory_Is_Null() {
            // Arrange
            var result = Result.Fail("error");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                result.ToProblemDetailsResult(null!, httpContext));
        }

        [Fact]
        public void ToProblemDetailsResult_ThrowsArgumentNullException_When_HttpContext_Is_Null() {
            // Arrange
            var result = Result.Fail("error");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                result.ToProblemDetailsResult(mockFactory.Object, null!));
        }

        [Fact]
        public void ToProblemDetailsResult_Generic_ThrowsArgumentNullException_When_Result_Is_Null() {
            // Arrange
            Result<int> result = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                result.ToProblemDetailsResult(mockFactory.Object, httpContext));
        }

        [Fact]
        public void ToProblemDetailsResult_Generic_ThrowsArgumentNullException_When_ProblemDetailsFactory_Is_Null() {
            // Arrange
            var result = Result.Fail<int>("error");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                result.ToProblemDetailsResult(null!, httpContext));
        }

        [Fact]
        public void ToProblemDetailsResult_Generic_ThrowsArgumentNullException_When_HttpContext_Is_Null() {
            // Arrange
            var result = Result.Fail<int>("error");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                result.ToProblemDetailsResult(mockFactory.Object, null!));
        }

        // ---- Successful result guard ----

        [Fact]
        public void ToProblemDetailsResult_ThrowsInvalidOperationException_For_Successful_Result() {
            // Arrange
            var result = Result.Ok();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                result.ToProblemDetailsResult(mockFactory.Object, httpContext));
        }

        [Fact]
        public void ToProblemDetailsResult_Generic_ThrowsInvalidOperationException_For_Successful_Result() {
            // Arrange
            var result = Result.Ok(42);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                result.ToProblemDetailsResult(mockFactory.Object, httpContext));
        }

        [Fact]
        public void ToProblemDetailsResult_ThrowsInvalidOperationException_With_Correct_Message_For_Successful_Result() {
            // Arrange
            var result = Result.Ok();

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() =>
                result.ToProblemDetailsResult(mockFactory.Object, httpContext));

            // Assert
            Assert.Equal("Can not create ProblemDetails result from a successful fluent result.", ex.Message);
        }

        // ---- ValidationError mapping ----

        [Fact]
        public void ToProblemDetailsResult_Returns_BadRequestObjectResult_For_ValidationError() {
            // Arrange
            var result = Result.Fail(new ValidationError("Name", "Name is required."));

            // Act
            var actionResult = result.ToProblemDetailsResult(mockFactory.Object, httpContext);

            // Assert
            Assert.IsType<BadRequestObjectResult>(actionResult);
        }

        [Fact]
        public void ToProblemDetailsResult_Returns_Validation_Errors_Grouped_By_PropertyName() {
            // Arrange
            var result = Result.Fail(new List<IError> {
                new ValidationError("Name", "Name is required."),
                new ValidationError("Name", "Name is too short."),
                new ValidationError("Email", "Email is invalid.")
            });

            // Act
            var actionResult = result.ToProblemDetailsResult(mockFactory.Object, httpContext);

            // Assert
            var problemDetails = Assert.IsType<ValidationProblemDetails>(actionResult.Value);
            Assert.Equal(2, problemDetails.Errors.Count);
            Assert.Equal(new[] { "Name is required.", "Name is too short." }, problemDetails.Errors["Name"]);
            Assert.Equal(new[] { "Email is invalid." }, problemDetails.Errors["Email"]);
        }

        [Fact]
        public void ToProblemDetailsResult_Uses_EmptyString_Key_For_ValidationError_With_Null_PropertyName() {
            // Arrange
            var result = Result.Fail(new ValidationError(null!, "Object-level error."));

            // Act
            var actionResult = result.ToProblemDetailsResult(mockFactory.Object, httpContext);

            // Assert
            var problemDetails = Assert.IsType<ValidationProblemDetails>(actionResult.Value);
            Assert.True(problemDetails.Errors.ContainsKey(string.Empty));
            Assert.Equal(new[] { "Object-level error." }, problemDetails.Errors[string.Empty]);
        }

        // ---- HttpStatusCodeAppError mapping ----

        [Theory]
        [InlineData(HttpStatusCode.BadRequest, typeof(BadRequestObjectResult))]
        [InlineData(HttpStatusCode.NotFound, typeof(NotFoundObjectResult))]
        [InlineData(HttpStatusCode.Unauthorized, typeof(UnauthorizedObjectResult))]
        public void ToProblemDetailsResult_Returns_Correct_ObjectResult_Type_For_HttpStatusCodeAppError(
            HttpStatusCode statusCode,
            Type expectedResultType) {

            // Arrange
            var result = Result.Fail(new HttpStatusCodeAppError(statusCode, "error"));

            mockFactory
                .Setup(f => f.CreateProblemDetails(
                    It.IsAny<HttpContext>(),
                    (int)statusCode,
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>()))
                .Returns(new ProblemDetails { Status = (int)statusCode });

            // Act
            var actionResult = result.ToProblemDetailsResult(mockFactory.Object, httpContext);

            // Assert
            Assert.IsType(expectedResultType, actionResult);
        }

        [Fact]
        public void ToProblemDetailsResult_Returns_ObjectResult_For_Custom_HttpStatusCode() {
            // Arrange
            var result = Result.Fail(new HttpStatusCodeAppError(HttpStatusCode.ServiceUnavailable, "Service unavailable."));

            mockFactory
                .Setup(f => f.CreateProblemDetails(
                    It.IsAny<HttpContext>(),
                    (int)HttpStatusCode.ServiceUnavailable,
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>()))
                .Returns(new ProblemDetails { Status = (int)HttpStatusCode.ServiceUnavailable });

            // Act
            var actionResult = result.ToProblemDetailsResult(mockFactory.Object, httpContext);

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal((int)HttpStatusCode.ServiceUnavailable, actionResult.StatusCode);
        }

        [Fact]
        public void ToProblemDetailsResult_Uses_Message_As_Detail_For_HttpStatusCodeAppError() {
            // Arrange
            const string expectedDetail = "Resource not found.";
            var result = Result.Fail(new HttpStatusCodeAppError(HttpStatusCode.NotFound, expectedDetail));

            mockFactory
                .Setup(f => f.CreateProblemDetails(
                    It.IsAny<HttpContext>(),
                    (int)HttpStatusCode.NotFound,
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    expectedDetail,
                    It.IsAny<string?>()))
                .Returns(new ProblemDetails { Status = (int)HttpStatusCode.NotFound, Detail = expectedDetail });

            // Act
            var actionResult = result.ToProblemDetailsResult(mockFactory.Object, httpContext);

            // Assert
            var problemDetails = Assert.IsType<ProblemDetails>(actionResult.Value);
            Assert.Equal(expectedDetail, problemDetails.Detail);
        }

        [Fact]
        public void ToProblemDetailsResult_Does_Not_Add_Error_Extension_For_Result_With_Only_HttpStatusCodeAppError() {
            // Arrange
            var result = Result.Fail(new HttpStatusCodeAppError(HttpStatusCode.NotFound, "Not found."));

            // Act
            var actionResult = result.ToProblemDetailsResult(mockFactory.Object, httpContext);

            // Assert
            var problemDetails = Assert.IsType<ProblemDetails>(actionResult.Value);
            Assert.False(problemDetails.Extensions.ContainsKey("error"));
        }

        [Theory]
        [MemberData(nameof(MixedErrorCombinations))]
        public void ToProblemDetailsResult_Adds_NonHttpStatusCode_Errors_To_Extension_When_Result_Has_Mixed_Errors(IError extraError) {
            // Arrange
            var result = Result.Fail(new List<IError>
            {
                new HttpStatusCodeAppError(HttpStatusCode.NotFound, "Not found."),
                extraError
            });

            // Act
            var actionResult = result.ToProblemDetailsResult(mockFactory.Object, httpContext);

            // Assert
            Assert.IsType<NotFoundObjectResult>(actionResult);
            var problemDetails = Assert.IsType<ProblemDetails>(actionResult.Value);
            Assert.True(problemDetails.Extensions.ContainsKey("error"));
            var errors = Assert.IsAssignableFrom<IEnumerable<IError>>(problemDetails.Extensions["error"]);
            Assert.Contains(extraError, errors);
        }

        // ---- Generic error mapping ----

        [Fact]
        public void ToProblemDetailsResult_Populates_Error_Extension_For_Generic_Errors() {
            // Arrange
            var result = Result.Fail("Something went wrong.");

            // Act
            var actionResult = result.ToProblemDetailsResult(mockFactory.Object, httpContext);

            // Assert
            var problemDetails = Assert.IsType<ProblemDetails>(actionResult.Value);
            Assert.True(problemDetails.Extensions.ContainsKey("error"));
        }

        // ---- Instance population ----

        [Fact]
        public void ToProblemDetailsResult_Instance_Includes_Path_And_QueryString() {
            // Arrange
            httpContext.Request.Path = "/api/resource";
            httpContext.Request.QueryString = new QueryString("?id=5");

            var result = Result.Fail("error");

            // Act
            var actionResult = result.ToProblemDetailsResult(mockFactory.Object, httpContext);

            // Assert
            var problemDetails = Assert.IsType<ProblemDetails>(actionResult.Value);
            Assert.Equal("/api/resource?id=5", problemDetails.Instance);
        }

        [Fact]
        public void ToProblemDetailsResult_Instance_Uses_Path_Only_When_No_QueryString() {
            // Arrange
            httpContext.Request.Path = "/api/resource";
            httpContext.Request.QueryString = QueryString.Empty;

            var result = Result.Fail("error");

            // Act
            var actionResult = result.ToProblemDetailsResult(mockFactory.Object, httpContext);

            // Assert
            var problemDetails = Assert.IsType<ProblemDetails>(actionResult.Value);
            Assert.Equal("/api/resource", problemDetails.Instance);
        }

        #endregion
    }
}
