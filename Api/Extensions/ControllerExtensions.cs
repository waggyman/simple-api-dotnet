using Microsoft.AspNetCore.Mvc;

namespace simple_api.Api.Extensions;

public static class ControllerExtensions
{
    public static bool TryGetUserId(this ControllerBase controller, out int userId)
    {
        var id = controller.User.GetUserId();

        if (id is null)
        {
            userId = default;
            return false;
        }

        userId = id.Value;
        return true;
    }
}
