using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MvPresentation.Hubs;

[Authorize]
public class UserHub : Hub;
