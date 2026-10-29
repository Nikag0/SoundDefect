using ScottPlot;
using SoundDefect.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SoundDefect.View
{
    /// <summary>
    /// Содержимое одной вкладки: список сигналов, лазер, настройки отображения и график.
    /// </summary>
    public partial class ChartTabView : UserControl
    {
        // Одинаковые отступы слева и справа, чтобы область данных графика и полосы лазера совпадала по X
        private const float PlotPaddingLeft = 70;
        private const float PlotPaddingRight = 20;

        private SignalPlotRenderer? _plotRenderer;
        private LaserIntervalsRenderer? _laserRenderer;
        private PlotScaleController? _scaleController;

        public ChartTabView()
        {
            InitializeComponent();
            ConfigurePlots();

            // Отрисовщики живут, пока представление находится в окне.
            // Скрытие неактивной вкладки (Visibility) не вызывает Unloaded, поэтому график не пересоздаётся.
            Loaded += (_, _) => AttachRenderers();
            Unloaded += (_, _) => DetachRenderers();
            DataContextChanged += (_, _) =>
            {
                if (!IsLoaded) return;
                DetachRenderers();
                AttachRenderers();
            };
        }

        private void ConfigurePlots()
        {
            Plot.Plot.Layout.Fixed(new PixelPadding(PlotPaddingLeft, PlotPaddingRight, 40, 10));

            // Полоса лазера: без осей и сетки, ось X повторяет основной график, мышью не управляется
            LaserStrip.Plot.Layout.Fixed(new PixelPadding(PlotPaddingLeft, PlotPaddingRight, 2, 2));
            LaserStrip.Plot.HideAxesAndGrid();
            LaserStrip.Plot.DataBackground.Color = Colors.LightGray.WithAlpha(0.4);
            LaserStrip.UserInputProcessor.Disable();

            Plot.Plot.Axes.Link(LaserStrip, x: true, y: false);
        }

        /// <summary>
        /// Enter в поле ввода сразу применяет значение, не дожидаясь ухода фокуса.
        /// </summary>
        private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || sender is not TextBox textBox) return;

            textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            textBox.SelectAll();
            e.Handled = true;
        }

        private void AttachRenderers()
        {
            if (_plotRenderer != null || DataContext is not ChartTabViewModel tab) return;

            _plotRenderer = new SignalPlotRenderer(Plot, tab.PlotSignals, tab.Scale);
            _laserRenderer = new LaserIntervalsRenderer(Plot, LaserStrip, tab);
            _scaleController = new PlotScaleController(Plot, tab.Scale);
        }

        private void DetachRenderers()
        {
            _scaleController?.Dispose();
            _scaleController = null;

            _plotRenderer?.Dispose();
            _plotRenderer = null;

            _laserRenderer?.Dispose();
            _laserRenderer = null;
        }
    }
}
