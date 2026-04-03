using L2.Application.Exceptions;
using MediatR;
using MvApplication.Repositories;
using MvDomain.Entities;

namespace MvApplication.UseCases.UpdateUserPoint;

public class UpdateUserPointHandler(
  IRepository<User> userRepository
) : IRequestHandler<UpdateUserPointCommand, UpdateUserPointResult> {
  public async Task<UpdateUserPointResult> Handle(UpdateUserPointCommand request, CancellationToken ct) {
    var user = await userRepository.GetByIdAsync(request.UserId, ct);
    if (user == null) {
      throw new WorkflowException($"User không tồn tại: {request.UserId}", 404);
    }

    user.AddPoint(request.Point);

    await userRepository.UpdateAsync(user, ct);

    return new UpdateUserPointResult(
      user.Id,
      user.CumulativePoint
    );
  }
}
