using LibrarySystem.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Api;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this ControllerBase controller, Result result)
    {
        if (result.IsSuccess)
        {
            return controller.NoContent();
        }

        return controller.ToErrorResult(result.Error!);
    }

    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        return controller.ToErrorResult(result.Error!);
    }

    public static IActionResult ToCreatedResult<T>(this ControllerBase controller, string location, Result<T> result)
    {
        if (result.IsSuccess)
        {
            return controller.Created(location, result.Value);
        }

        return controller.ToErrorResult(result.Error!);
    }

    private static IActionResult ToErrorResult(this ControllerBase controller, Error error)
    {
        var status = error.Code switch
        {
            "INVALID_CREDENTIALS" or "UNAUTHORIZED" => StatusCodes.Status401Unauthorized,
            "FORBIDDEN" => StatusCodes.Status403Forbidden,
            "NOT_FOUND" => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest
        };

        return controller.StatusCode(status, new
        {
            error = new
            {
                code = error.Code,
                message = error.Message,
                details = error.Details
            }
        });
    }
}
