﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Astral.Plane.Controls
{
    /// <summary>
    /// Simple numeric updown so we don't have to use that nasty winforms control
    /// </summary>
    public partial class NumericUpDownEx : UserControl
    {
        private int _startValue = 0;
        private int _lastDelta = 0;
        const int ZERO_WINDOW = 7;

        public NumericUpDownEx()
        {
            ScaleFactor = 4;
            InitializeComponent();

            this.PreviewKeyDown += new KeyEventHandler(NumericUpDownEx_PreviewKeyDown);
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            bool useHoriz = Math.Abs(e.HorizontalChange) > Math.Abs(e.VerticalChange);
            _lastDelta = (int)Math.Round(useHoriz ? e.HorizontalChange : -e.VerticalChange);
            if ((useHoriz ? Math.Abs(e.HorizontalChange) : Math.Abs(e.VerticalChange)) < ZERO_WINDOW)
            {
                // Make returning to 0 easy
                _lastDelta = 0;
            }
            else
            {
                int zeroWindow = _lastDelta > 0 ? ZERO_WINDOW : -ZERO_WINDOW;
                _lastDelta = (_lastDelta - zeroWindow) / ScaleFactor;
            }
            SetValue();
        }

        private void SetValue()
        {
            this.Value = Math.Min(Math.Max(this.Minimum, _lastDelta + _startValue), this.Maximum);
        }

        public int ScaleFactor { get; set; }

        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(int), typeof(NumericUpDownEx), new UIPropertyMetadata(int.MinValue));

        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(int), typeof(NumericUpDownEx), new UIPropertyMetadata(int.MaxValue));


        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(NumericUpDownEx), new UIPropertyMetadata(0, new PropertyChangedCallback(ValueChangedCallback), new CoerceValueCallback(CoerceValue)));

        private static void ValueChangedCallback(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            NumericUpDownEx nud = (NumericUpDownEx)target;

            if (nud.ValueChanged != null)
                nud.ValueChanged(nud, (int)args.NewValue);
        }

        private static object CoerceValue(DependencyObject target, object value)
        {
            if (target == null || value == null) return value;

            int val = (int)value;
            NumericUpDownEx nud = (NumericUpDownEx)target;
            return Math.Min(Math.Max(nud.Minimum, val), nud.Maximum);
        }

        private void Thumb_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _startValue = Value;
        }

        private void Thumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            SetValue();
        }

        public event Action<object, int> ValueChanged;



        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            this.Value += e.Delta > 0 ? 1 : -1;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.SelectAll();
        }

        void NumericUpDownEx_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                Value++;
            }
            else if (e.Key == Key.Down)
            {
                Value--;
            }
        }
    }
}
