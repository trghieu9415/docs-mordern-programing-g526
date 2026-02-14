using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvApplication.UseCases.Auth.GetProfile;
using MvApplication.UseCases.Auth.Login;
using MvApplication.UseCases.Auth.Logout;
using MvApplication.UseCases.Auth.Refresh;
using MvApplication.UseCases.Auth.Register;
using MvPresentation.Response;

namespace MvPresentation.Controllers.User;

[ApiController]
[Route("api/user/auth")]
[ApiExplorerSettings(GroupName = "user")]
public class AuthController(IMediator mediator) : ControllerBase
{

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await mediator.Send(command with { Role = MvApplication.Models.UserRole.User });
        return AppResponse.Success(result, "Đăng ký thành công");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await mediator.Send(command);
        return AppResponse.Success(result, "Đăng nhập thành công");
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshCommand command)
    {
        var result = await mediator.Send(command);
        return AppResponse.Success(result, "Làm mới token thành công");
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
    {
        await mediator.Send(command);
        return AppResponse.Success("Đăng xuất thành công");
    }

    [HttpGet("profile")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await mediator.Send(new GetProfileQuery());
        return AppResponse.Success(result, "Lấy thông tin thành công");
    }
}
