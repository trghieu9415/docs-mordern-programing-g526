using MvApplication.Models;

namespace MvApplication.Ports.Security;

public interface ICurrentUser
{
    User User { get; }
}
