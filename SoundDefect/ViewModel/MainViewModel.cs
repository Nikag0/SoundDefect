using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SoundDefect.ViewModel
{
    /// <summary>
    /// Главное окно: список вкладок с графиками, выбранная вкладка, добавление и закрытие вкладок.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly ChartTabFactory _tabFactory;
        private const string TabTitlePrefix = "График ";

        private ChartTabViewModel? _selectedTab;

        public MainViewModel(ChartTabFactory tabFactory)
        {
            _tabFactory = tabFactory;

            AddTabCommand = new RelayCommand(AddTab);
            CloseTabCommand = new RelayCommand<ChartTabViewModel>(CloseTab, _ => Tabs.Count > 1);

            AddTab();
        }

        public ObservableCollection<ChartTabViewModel> Tabs { get; } = new();

        public ChartTabViewModel? SelectedTab
        {
            get => _selectedTab;
            set
            {
                var previous = _selectedTab;
                if (!SetField(ref _selectedTab, value)) return;

                if (previous != null) previous.IsSelected = false;
                if (value != null) value.IsSelected = true;
            }
        }

        public ICommand AddTabCommand { get; }

        /// <summary>
        /// Закрывает вкладку. Последнюю вкладку закрыть нельзя.
        /// </summary>
        public ICommand CloseTabCommand { get; }

        private void AddTab()
        {
            var tab = _tabFactory.Create(TabTitlePrefix + NextFreeTabNumber());

            Tabs.Add(tab);
            SelectedTab = tab;
        }

        /// <summary>
        /// Наименьший номер, не занятый открытыми вкладками: номера закрытых вкладок используются повторно.
        /// </summary>
        private int NextFreeTabNumber()
        {
            var usedTitles = Tabs.Select(t => t.Title).ToHashSet();

            int number = 1;
            while (usedTitles.Contains(TabTitlePrefix + number))
                number++;

            return number;
        }

        private void CloseTab(ChartTabViewModel tab)
        {
            int index = Tabs.IndexOf(tab);
            if (index < 0 || Tabs.Count <= 1) return;

            // После закрытия выбранной вкладки выбирается соседняя справа, а если её нет — слева
            if (tab == SelectedTab)
                SelectedTab = index + 1 < Tabs.Count ? Tabs[index + 1] : Tabs[index - 1];

            Tabs.Remove(tab);
        }
    }
}
