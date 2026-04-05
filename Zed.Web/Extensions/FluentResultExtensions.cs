using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using Zed.Errors;

namespace Zed.Web.Extensions {
    /// <summary>
    /// Provides extension methods for converting <see cref="Result"/> instances into ASP.NET Core problem details responses.
    /// </summary>
    public static class FluentResultExtensions {

        private const string ErrorExtension = "error";

        #region Extension methods

        /// <summary>
        /// Converts a failed <see cref="Result"/> into an <see cref="ObjectResult"/> containing problem details.
        /// </summary>
        /// <param name="result">The result to convert.</param>
        /// <param name="problemDetailsFactory">The factory used to create problem details instances.</param>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <returns>An <see cref="ObjectResult"/> representing the failed result.</returns>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="result"/> is successful.</exception>
        public static ObjectResult ToProblemDetailsResult(this Result result, ProblemDetailsFactory problemDetailsFactory, HttpContext httpContext) {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(problemDetailsFactory);
            ArgumentNullException.ThrowIfNull(httpContext);

            if (result.IsSuccess) {
                throw new InvalidOperationException("Can not create ProblemDetails result from a successful fluent result.");
            }

            ProblemDetails problemDetails;

            if (result.HasError<HttpStatusCodeAppError>(out var httpStatusCodeErrors)) {
                problemDetails = CreateHttpStatusCodeProblemDetails(problemDetailsFactory, httpContext, result, httpStatusCodeErrors);
            } else if (result.HasError<ValidationError>(out var validationErrors)) {
                problemDetails = CreateValidationProblemDetails(problemDetailsFactory, httpContext, validationErrors);
            } else {
                problemDetails = CreateProblemDetails(problemDetailsFactory, httpContext, result);
            }

            problemDetails.Instance = $"{httpContext?.Request?.Path}{httpContext?.Request?.QueryString}";

            return (HttpStatusCode?)problemDetails.Status switch {
                HttpStatusCode.BadRequest => new BadRequestObjectResult(problemDetails),
                HttpStatusCode.NotFound => new NotFoundObjectResult(problemDetails),
                HttpStatusCode.Unauthorized => new UnauthorizedObjectResult(problemDetails),
                _ => new ObjectResult(problemDetails) { StatusCode = problemDetails.Status }
            };
        }

        /// <summary>
        /// Converts a failed <see cref="Result{T}"/> into an <see cref="ObjectResult"/> containing problem details.
        /// </summary>
        /// <typeparam name="T">The type of the result value.</typeparam>
        /// <param name="result">The result to convert.</param>
        /// <param name="problemDetailsFactory">The factory used to create problem details instances.</param>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <returns>An <see cref="ObjectResult"/> representing the failed result.</returns>
        public static ObjectResult ToProblemDetailsResult<T>(this Result<T> result, ProblemDetailsFactory problemDetailsFactory, HttpContext httpContext) {
            ArgumentNullException.ThrowIfNull(result);
            return result.ToResult().ToProblemDetailsResult(problemDetailsFactory, httpContext);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a default <see cref="ProblemDetails"/> instance for a failed result.
        /// </summary>
        /// <param name="problemDetailsFactory">The factory used to create problem details instances.</param>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="result">The failed result.</param>
        /// <returns>A <see cref="ProblemDetails"/> instance containing the result errors.</returns>
        private static ProblemDetails CreateProblemDetails(ProblemDetailsFactory problemDetailsFactory, HttpContext httpContext, Result result) {
            var problemDetails = problemDetailsFactory.CreateProblemDetails(httpContext);
            problemDetails.Extensions.Add(ErrorExtension, result.Errors);
            return problemDetails;
        }

        /// <summary>
        /// Creates a <see cref="ValidationProblemDetails"/> instance from validation errors.
        /// </summary>
        /// <param name="problemDetailsFactory">The factory used to create problem details instances.</param>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="validationErrors">The validation errors to include.</param>
        /// <returns>A <see cref="ValidationProblemDetails"/> instance populated with validation errors.</returns>
        private static ValidationProblemDetails CreateValidationProblemDetails(
            ProblemDetailsFactory problemDetailsFactory,
            HttpContext httpContext,
            IEnumerable<ValidationError> validationErrors) {

            var validationErrorsDict = validationErrors
                .Select(validationError => (
                    PropertyName: validationError.PropertyName ?? string.Empty,
                    Message: validationError.Message))
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Message).ToArray());

            var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(httpContext, new ModelStateDictionary());

            problemDetails.Errors = validationErrorsDict;

            return problemDetails;

        }

        /// <summary>
        /// Creates a <see cref="ProblemDetails"/> instance using the first HTTP status code error found in the result.
        /// </summary>
        /// <param name="problemDetailsFactory">The factory used to create problem details instances.</param>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="result">The failed result.</param>
        /// <param name="httpStatusCodeErrors">The HTTP status code errors associated with the result.</param>
        /// <returns>A <see cref="ProblemDetails"/> instance populated from the HTTP status code error.</returns>
        private static ProblemDetails CreateHttpStatusCodeProblemDetails(
            ProblemDetailsFactory problemDetailsFactory,
            HttpContext httpContext,
            Result result,
            IEnumerable<HttpStatusCodeAppError> httpStatusCodeErrors) {

            var httpStatusCodeError = httpStatusCodeErrors.First();

            var problemDetails = problemDetailsFactory.CreateProblemDetails(
                httpContext,
                statusCode: httpStatusCodeError.Code,
                detail: httpStatusCodeError.Message);

            if (result.HasError(x => x.GetType() != typeof(HttpStatusCodeAppError), out var errors)) {
                problemDetails.Extensions.Add(ErrorExtension, errors);
            }

            return problemDetails;

        }

        #endregion

    }
}
