namespace MvApplication.Abstractions;

public interface ILockable {
  string LockKey { get; }
  TimeSpan WaitTime { get; }
}
