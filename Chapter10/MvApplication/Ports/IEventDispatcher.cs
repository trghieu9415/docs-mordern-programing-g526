namespace MvApplication.Ports;

public interface IEventDispatcher {
  Task DispatchEventsAsync(CancellationToken ct = default);
}
