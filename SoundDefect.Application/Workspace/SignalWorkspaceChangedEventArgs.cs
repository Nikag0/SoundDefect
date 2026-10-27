using SoundDefect.Domain.Signals;

namespace SoundDefect.Application.Workspace
{
    public enum SignalWorkspaceChange
    {
        Added,
        Removed,
        Cleared
    }

    public class SignalWorkspaceChangedEventArgs : EventArgs
    {
        public SignalWorkspaceChange Change { get; }

        /// <summary>
        /// Сигналы, которых коснулось изменение (для Cleared — все удалённые).
        /// </summary>
        public IReadOnlyList<Signal> Signals { get; }

        public SignalWorkspaceChangedEventArgs(SignalWorkspaceChange change, IReadOnlyList<Signal> signals)
        {
            Change = change;
            Signals = signals;
        }
    }
}
