using System;
using System.Collections.Generic;
using System.Text;

namespace SoundDefect.Domain
{
    public class Graphic
    {
        public int Id { get; }
        public string Name { get; private set; }
        public int Color { get; private set; }

        public double[] XPositions { get; }
        public double[] YValuesOriginal { get; }

        public double[]? YValuesNormalized { get; private set; }

        private Graphic(int id, string name, double[] xs, double[] ysOriginal, int color)
        {
            Id = id;
            Name = name;
            XPositions = xs;
            YValuesOriginal = ysOriginal;
            Color = color;
        }

        public static Graphic Create(int id, string name, double[] xs, double[] ys, int color)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "Идентификатор графика должен быть больше нуля.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя графика не может быть пустым или состоять из пробелов.", nameof(name));

            if (xs == null || ys == null)
                throw new ArgumentNullException(xs == null ? nameof(xs) : nameof(ys), "Массивы данных не могут быть null.");

            if (xs.Length == 0 || ys.Length == 0)
                throw new ArgumentException("График должен содержать хотя бы одну точку данных.");

            if (xs.Length != ys.Length)
                throw new ArgumentException("Количество координат X должно совпадать с количеством координат Y.");

            if (color == 0)
                throw new ArgumentException("Указан невалидный или пустой цвет графика.", nameof(color));

            double[] xsCopy = (double[])xs.Clone();
            double[] ysOriginalCopy = (double[])ys.Clone();

            return new Graphic(id, name, xsCopy, ysOriginalCopy, color);
        }

        public void ChangeName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Новое имя не может быть пустым.", nameof(newName));

            Name = newName;
        }

        public void ChangeColor(int newColor)
        {
            if (newColor == 0)
                throw new ArgumentException("Недопустимое значение цвета.", nameof(newColor));

            Color = newColor;
        }

        public void UpdateNormalizedData(double[] normalizedYs)
        {
            if (normalizedYs == null)
                throw new ArgumentNullException(nameof(normalizedYs));
            if (normalizedYs.Length != YValuesOriginal.Length)
                throw new ArgumentException("Размер нормализованного массива должен совпадать с оригинальным.");

            YValuesNormalized = normalizedYs;
        }
    }
}
