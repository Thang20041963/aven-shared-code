using Microsoft.AspNetCore.Mvc;
using SharedCode.Contracts.Results;
namespace SharedCode.Api.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult(
            this ControllerBase controller,
            Result result)
        {
            return result.ErrorType switch
            {
                ErrorType.Validation =>
                    controller.BadRequest(result),

                ErrorType.NotFound =>
                    controller.NotFound(result),

                ErrorType.Conflict =>
                    controller.Conflict(result),

                ErrorType.Unauthorized =>
                    controller.Unauthorized(),

                ErrorType.Forbidden =>
                    controller.Forbid(),

                _ =>
                    controller.StatusCode(500, result)
            };
        }

       public static IActionResult ToActionResult<T>(
       this ControllerBase controller,
       Result<T> result)
        {
            if (result.IsSuccess)
            {
                return controller.Ok(result);
            }

            return result.ErrorType switch
            {
                ErrorType.Validation =>
                    controller.BadRequest(result),

                ErrorType.NotFound =>
                    controller.NotFound(result),

                ErrorType.Conflict =>
                    controller.Conflict(result),

                ErrorType.Unauthorized =>
                    controller.Unauthorized(),

                ErrorType.Forbidden =>
                    controller.Forbid(),

                _ =>
                    controller.StatusCode(500, result)
            };
        }
    }
}
