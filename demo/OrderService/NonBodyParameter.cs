using Microsoft.OpenApi.Models;

namespace OrderService
{
    public class NonBodyParameter : OpenApiParameter
    {
        public object Default { get; set; }
    }
}