using Microsoft.AspNetCore.Http;
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
                    controller.StatusCode(StatusCodes.Status401Unauthorized, result),

                ErrorType.Forbidden =>
                    controller.StatusCode(StatusCodes.Status403Forbidden, result),

                _ =>
                    controller.StatusCode(StatusCodes.Status500InternalServerError, result)
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
                    controller.StatusCode(StatusCodes.Status401Unauthorized, result),

                ErrorType.Forbidden =>
                    controller.StatusCode(StatusCodes.Status403Forbidden, result),

                _ =>
                    controller.StatusCode(StatusCodes.Status500InternalServerError, result)
            };
        }
    }
}
