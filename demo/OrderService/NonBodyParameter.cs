using Microsoft.OpenApi;

namespace OrderService
{
    /// <summary>
    /// Parameter without body.
    /// </summary>
    public class NonBodyParameter : OpenApiParameter
    {
        /// <summary>
        /// Default.
        /// </summary>
        public object Default { get; set; }
    }
}
