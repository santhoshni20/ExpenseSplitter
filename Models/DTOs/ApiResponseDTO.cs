using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ExpenseSplitter.Models.DTOs
{
    public class ApiResponseDTO : IActionResult
    {
        public int statusCode { get; set; } = 200;
        public string message { get; set; } = string.Empty;
        public object? data { get; set; }
        public object? errorDetails { get; set; }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var httpContext = context.HttpContext;
            var acceptHeader = httpContext.Request.Headers["Accept"].ToString();
            bool isHtmlRequest = acceptHeader.Contains("text/html", StringComparison.OrdinalIgnoreCase);

            // If browser requested HTML on a GET request, render the MVC View
            if (isHtmlRequest && httpContext.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                var viewResult = new ViewResult
                {
                    ViewName = "Index",
                    ViewData = new ViewDataDictionary<ApiResponseDTO>(
                        new EmptyModelMetadataProvider(),
                        new ModelStateDictionary())
                    {
                        Model = this
                    }
                };
                await viewResult.ExecuteResultAsync(context);
                return;
            }

            // Return standardized JSON response
            var jsonResult = new JsonResult(this)
            {
                StatusCode = statusCode
            };
            await jsonResult.ExecuteResultAsync(context);
        }
    }
}
