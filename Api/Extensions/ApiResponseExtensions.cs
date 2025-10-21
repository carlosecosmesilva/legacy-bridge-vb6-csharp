using Api.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions
{
    public static class ApiResponseExtensions
    {
        public static IActionResult ToActionResult<T>(this ApiResponse<T> response)
        {
            if (response.Success)
                return new OkObjectResult(response);
            
            if (response.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                return new NotFoundObjectResult(response);

            return new BadRequestObjectResult(response);
        }
    }
}
