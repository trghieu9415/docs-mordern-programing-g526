using MvDomain.Entities;

namespace MvApplication.Ports;

public interface IJwtService {
  string GenerateToken(User user);
}
