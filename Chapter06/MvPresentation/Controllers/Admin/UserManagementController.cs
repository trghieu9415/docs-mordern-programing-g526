using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvApplication.UseCases.User.GetAllUsers;
using MvPresentation.Response;

namespace MvPresentation.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[ApiExplorerSettings(GroupName = "admin")]
[Authorize(Roles = "Admin")]
public class UserManagementController(IMediator mediator) : ControllerBase
{

    /// <summary>
    /// Lấy danh sách tất cả người dùng (chỉ Admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await mediator.Send(new GetAllUsersQuery());
        return AppResponse.Success(result, "Lấy danh sách người dùng thành công");
    }
}
