using System.Security.Claims;
using L2.Application.Exceptions;
using MvApplication.Ports;

namespace MvPresentation.Adapters;

public class CurrentUser : ICurrentUser {
  public CurrentUser(IHttpContextAccessor accessor) {
    var user = accessor.HttpContext?.User;
    var id = user?.FindFirstValue(ClaimTypes.NameIdentifier);

    if (id != null) {
      Id = Guid.Parse(id);
      Username = user?.Identity?.Name ?? "Guest";
      Email = user?.FindFirstValue(ClaimTypes.Email) ?? "Guest";
    } else {
      throw new WorkflowException("Token không hợp lệ", 401);
    }
  }

  public Guid Id { get; }
  public string Username { get; }
  public string Email { get; }
}
