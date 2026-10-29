using SoundDefect.Domain.Signals;

namespace SoundDefect.Application.Workspace
{
    /// <summary>
    /// Хранит сигналы в памяти. Рассчитан на работу из UI-потока.
    /// </summary>
    public class SignalWorkspace : ISignalWorkspace
    {
        private readonly Dictionary<SignalId, Signal> _signals = new();

        public IReadOnlyCollection<Signal> Signals => _signals.Values;

        public Signal? Laser { get; private set; }

        public event EventHandler<SignalWorkspaceChangedEventArgs>? Changed;

        public bool TryGet(SignalId id, out Signal signal) => _signals.TryGetValue(id, out signal!);

        public void Add(IReadOnlyList<Signal> signals)
        {
            ArgumentNullException.ThrowIfNull(signals);
            if (signals.Count == 0) return;

            foreach (var signal in signals)
            {
                if (_signals.ContainsKey(signal.Id))
                    throw new InvalidOperationException($"Сигнал с идентификатором {signal.Id} уже загружен.");
            }

            foreach (var signal in signals)
                _signals.Add(signal.Id, signal);

            Changed?.Invoke(this, new SignalWorkspaceChangedEventArgs(SignalWorkspaceChange.Added, signals));
        }

        public bool Remove(SignalId id)
        {
            if (!_signals.Remove(id, out var removed))
                return false;

            Changed?.Invoke(this, new SignalWorkspaceChangedEventArgs(SignalWorkspaceChange.Removed, new[] { removed }));
            return true;
        }

        public void Clear()
        {
            if (_signals.Count == 0) return;

            var removed = _signals.Values.ToArray();
            _signals.Clear();

            Changed?.Invoke(this, new SignalWorkspaceChangedEventArgs(SignalWorkspaceChange.Cleared, removed));
        }

        public void SetLaser(Signal laser)
        {
            ArgumentNullException.ThrowIfNull(laser);

            Laser = laser;
            Changed?.Invoke(this, new SignalWorkspaceChangedEventArgs(SignalWorkspaceChange.LaserSet, new[] { laser }));
        }

        public bool RemoveLaser()
        {
            if (Laser == null) return false;

            var removed = Laser;
            Laser = null;

            Changed?.Invoke(this, new SignalWorkspaceChangedEventArgs(SignalWorkspaceChange.LaserRemoved, new[] { removed }));
            return true;
        }
    }
}
