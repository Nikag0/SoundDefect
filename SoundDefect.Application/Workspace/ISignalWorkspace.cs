using SoundDefect.Domain.Signals;

namespace SoundDefect.Application.Workspace
{
    /// <summary>
    /// Хранилище сигналов, загруженных в приложение. Единственный источник правды о них.
    /// </summary>
    public interface ISignalWorkspace
    {
        IReadOnlyCollection<Signal> Signals { get; }

        bool TryGet(SignalId id, out Signal signal);

        void Add(IReadOnlyList<Signal> signals);
        bool Remove(SignalId id);
        void Clear();

        event EventHandler<SignalWorkspaceChangedEventArgs>? Changed;
    }
}
