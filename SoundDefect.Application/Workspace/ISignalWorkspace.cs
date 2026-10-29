using SoundDefect.Domain.Signals;

namespace SoundDefect.Application.Workspace
{
    /// <summary>
    /// Хранилище сигналов, загруженных в приложение. Единственный источник правды о них.
    /// Помимо обычных сигналов хранит один сигнал лазера, по которому определяются интервалы его работы.
    /// </summary>
    public interface ISignalWorkspace
    {
        IReadOnlyCollection<Signal> Signals { get; }

        /// <summary>Сигнал лазера или null, если лазер не загружен.</summary>
        Signal? Laser { get; }

        bool TryGet(SignalId id, out Signal signal);

        void Add(IReadOnlyList<Signal> signals);
        bool Remove(SignalId id);

        /// <summary>Удаляет все обычные сигналы. Лазер остаётся.</summary>
        void Clear();

        /// <summary>Устанавливает лазер, заменяя прежний, если он был.</summary>
        void SetLaser(Signal laser);
        bool RemoveLaser();

        event EventHandler<SignalWorkspaceChangedEventArgs>? Changed;
    }
}
