namespace SoundDefect.ViewModel
{
    /// <summary>
    /// Непрерывный кусок кривой на графике. Между отрезками одного сигнала рисуется разрыв.
    /// </summary>
    public sealed record SignalSegment(double[] Xs, double[] Ys);
}
