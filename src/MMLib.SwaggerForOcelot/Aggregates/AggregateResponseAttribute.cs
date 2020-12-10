using Kros.Utils;
using System;

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
        public AggregateResponseAttribute(string description, Type responseType)
        {
            Description = Check.NotNull(description, nameof(description));
            ResponseType = Check.NotNull(responseType, nameof(responseType));
        }

        /// <summary>
        /// Gets the aggregate response description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the type of the response.
        /// </summary>
        public Type ResponseType { get; private set; }
    }
}
