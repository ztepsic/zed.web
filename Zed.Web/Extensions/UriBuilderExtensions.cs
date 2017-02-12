using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;

namespace Zed.Web.Extensions {
    /// <summary>
    /// Uri extensions
    /// </summary>
    public static class UriBuilderExtensions {

        /// <summary>
        /// Build URL-encoded query string.
        /// Method appends given params to query the string.
        /// </summary>
        /// <param name="uriBuilder">UriBuilder</param>
        /// <param name="queryParams">Query params</param>
        /// <returns>Self</returns>
        public static UriBuilder BuildQueryString(this UriBuilder uriBuilder, NameValueCollection queryParams) {
            if (queryParams == null) return uriBuilder;


            var httpValueCollection = HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (var queryParamKey in queryParams.AllKeys) {
                httpValueCollection.Add(queryParamKey, queryParams[queryParamKey]);
            }

            uriBuilder.Query = httpValueCollection.ToString();

            return uriBuilder;
        }

        /// <summary>
        /// Build URL-encoded query string.
        /// Method appends given params to query the string.
        /// </summary>
        /// <param name="uriBuilder">UriBuilder</param>
        /// <param name="queryParams">Query params</param>
        /// <returns>Self</returns>
        public static UriBuilder BuildQueryString(this UriBuilder uriBuilder, object queryParams) {
            if (queryParams == null) return uriBuilder;

            var httpValueCollection = HttpUtility.ParseQueryString(uriBuilder.Query);

            foreach (var propertyInfo in queryParams.GetType().GetProperties()) {
                var value = propertyInfo.GetValue(queryParams);
                httpValueCollection.Add(propertyInfo.Name, value.ToString());
            }

            uriBuilder.Query = httpValueCollection.ToString();

            return uriBuilder;


        }

        /// <summary>
        /// Removes query params from query string
        /// </summary>
        /// <param name="uriBuilder">UriBuilder</param>
        /// <param name="queryParams">Query param names for removal</param>
        /// <returns>Self</returns>
        public static UriBuilder RemoveQueryParams(this UriBuilder uriBuilder, IEnumerable<string> queryParams) {
            if (queryParams == null) return uriBuilder;
            return uriBuilder.RemoveQueryParams(queryParams.ToArray());
        }


        /// <summary>
        /// Removes query params from query string
        /// </summary>
        /// <param name="uriBuilder">UriBuilder</param>
        /// <param name="queryParams">Query param names for removal</param>
        /// <returns>Self</returns>
        public static UriBuilder RemoveQueryParams(this UriBuilder uriBuilder, params string[] queryParams) {
            if (queryParams == null) return uriBuilder;
            var httpValueCollection = HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (var queryParam in queryParams) {
                httpValueCollection.Remove(queryParam);
            }

            uriBuilder.Query = httpValueCollection.ToString();
            return uriBuilder;
        }

        /// <summary>
        /// Removes query param from query string
        /// </summary>
        /// <param name="uriBuilder">UriBuilder</param>
        /// <param name="queryParam">Query param name for removal</param>
        /// <returns>Self</returns>
        public static UriBuilder RemoveQueryParam(this UriBuilder uriBuilder, string queryParam) {
            return string.IsNullOrEmpty(queryParam) ? uriBuilder : uriBuilder.RemoveQueryParams(queryParam);
        }

        /// <summary>
        /// Combines uri segments into one whole
        /// </summary>
        /// <param name="uriBuilder">UriBuilder</param>
        /// <param name="segments">URI segments to combine</param>
        /// <returns>Self</returns>
        public static UriBuilder PathCombine(this UriBuilder uriBuilder, params string[] segments) {
            if (segments == null || segments.Length == 0) return uriBuilder;
            uriBuilder.Path = combine(segments);
            return uriBuilder;
        }

        /// <summary>
        /// Combines uri segments into one whole
        /// </summary>
        /// <param name="uriBuilder">UriBuilder</param>
        /// <param name="segments">URI segments to combine</param>
        /// <returns>Self</returns>
        public static UriBuilder PathCombine(this UriBuilder uriBuilder, IEnumerable<string> segments) {
            return uriBuilder.PathCombine(segments.ToArray());
        }

        /// <summary>
        /// Appends a segment to the URI path.
        /// </summary>
        /// <param name="uriBuilder">UriBuilder</param>
        /// <param name="segment">The segment to append</param>
        /// <returns>Self</returns>
        public static UriBuilder AppendPathSegment(this UriBuilder uriBuilder, string segment) {
            if (string.IsNullOrEmpty(segment)) return uriBuilder;

            uriBuilder.Path = combine(uriBuilder.Path, segment);

            return uriBuilder;
        }

        /// <summary>
        /// Appends multiple segments to the URI path.
        /// </summary>
        /// <param name="uriBuilder">UriBuilder</param>
        /// <param name="segments">Segments to append</param>
        /// <returns>Self</returns>
        public static UriBuilder AppendPathSegments(this UriBuilder uriBuilder, params string[] segments) {
            if (segments == null) return uriBuilder;

            uriBuilder.Path = combine(uriBuilder.Path, combine(segments));

            return uriBuilder;
        }

        /// <summary>
        /// Appends multiple segments to the URI path.
        /// </summary>
        /// <param name="uriBuilder">UriBuilder</param>
        /// <param name="segments">Segments to append</param>
        /// <returns>Self</returns>
        public static UriBuilder AppendPathSegments(this UriBuilder uriBuilder, IEnumerable<string> segments) {
            return uriBuilder.AppendPathSegments(segments.ToArray());
        }

        /// <summary>
        /// Combines uri segments into one whole
        /// </summary>
        /// <param name="segments">path segments to combine</param>
        /// <returns>URI string</returns>
        private static string combine(params string[] segments) {
            if (segments == null) throw new ArgumentNullException(nameof(segments));

            StringBuilder stringBuilder = new StringBuilder();
            bool isFirstSegmentAdded = false;
            bool isInQueryString = false;
            bool isFirstParamAdded = false;
            foreach (var segment in segments) {
                if (string.IsNullOrEmpty(segment)) continue;

                if (segment.StartsWith("?") || segment.EndsWith("?")) {
                    if (isFirstSegmentAdded && !isInQueryString) stringBuilder.Append("/");
                    if (isInQueryString) {
                        stringBuilder.Append(segment.TrimStart('?'));
                        isFirstParamAdded = true;
                    } else {
                        stringBuilder.Append(segment);
                    }

                    if (!isInQueryString) isInQueryString = true;

                } else if (segment.StartsWith("#")) {
                    stringBuilder.Append(segment);
                } else if (isInQueryString) {
                    if (isFirstParamAdded) stringBuilder.Append("&");
                    stringBuilder.Append(segment.TrimStart('&').TrimEnd('&'));
                    if (!isFirstParamAdded) isFirstParamAdded = true;
                } else {
                    if (isFirstSegmentAdded) stringBuilder.Append("/");

                    stringBuilder.Append(segment.TrimStart('/').TrimEnd('/'));
                }

                if (!isFirstSegmentAdded) isFirstSegmentAdded = true;

            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Combines uri segments into one whole
        /// </summary>
        /// <param name="segments">URI segments to combine</param>
        /// <returns>An Uri instance</returns>
        public static Uri Combine(params string[] segments) {
            return new Uri(combine(segments));
        }


    }
}
