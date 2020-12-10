using Kros.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Mime;

namespace MMLib.SwaggerForOcelot.Aggregates
{
    /// <summary>
    /// Adds documentation for aggregate response when custom aggregator is used.
    /// Apply on your custom <see cref="IDefinedAggregator"/>.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AggregateResponseAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateResponseAggregate"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="responseType">Type of the response.</param>
        /// <param name="statusCode">Status code.</param>
        /// <param name="mediaType">Media type.</param>
        public AggregateResponseAttribute(
            string description,
            Type responseType,
            int statusCode = StatusCodes.Status200OK,
            string mediaType = MediaTypeNames.Application.Json)
        {
            Description = Check.NotNull(description, nameof(description));
            ResponseType = Check.NotNull(responseType, nameof(responseType));
            StatusCode = Check.GreaterOrEqualThan(statusCode, 0, nameof(statusCode));
            MediaType = Check.NotNull(mediaType, nameof(mediaType));
        }

        /// <summary>
        /// Gets the aggregate response description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the type of the response.
        /// </summary>
        public Type ResponseType { get; private set; }

        /// <summary>
        /// Gets the status code.
        /// </summary>
        public int StatusCode { get; private set; }

        /// <summary>
        /// Gets the type of the media.
        /// </summary>
        public string MediaType { get; private set; }
    }
}
