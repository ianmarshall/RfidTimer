using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace RaceTimer.Business
{
    public delegate void Invoker();

    public class TimerTextBlock : TextBlock
    {
        public event EventHandler OnCountDownComplete;

        private static event EventHandler OnTick;
        // ReSharper disable once UnusedMember.Local
        private static Timer _updateTimer = new Timer(UpdateTimer, null, 1000, 1000);
        private Invoker _updateTimeInvoker;

        public TimerTextBlock()
            : base()
        {
            Init();
        }

        public TimerTextBlock(Inline inline)
            : base(inline)
        {
            Init();
        }

        private void Init()
        {
            Loaded += TimerTextBlock_Loaded;
            Unloaded += TimerTextBlock_Unloaded;
        }

        ~TimerTextBlock()
        {
            Dispose();
        }

        public void Dispose()
        {
            OnTick -= TimerTextBlock_OnTick;
        }

        /// <summary>
        /// Represents the time remaining for the count down to complete if
        /// the control is initialized as a count down timer otherwise, it 
        /// represents the time elapsed since the timer has started.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// </exception>
        public TimeSpan TimeSpan
        {
            get { return (TimeSpan)GetValue(TimeSpanProperty); }
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentException();

                SetValue(TimeSpanProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for TimeSpan.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeSpanProperty =
            DependencyProperty.Register("TimeSpan", typeof(TimeSpan), typeof(TimerTextBlock), new UIPropertyMetadata(TimeSpan.Zero));


      

        public bool IsStarted
        {
            get { return (bool)GetValue(IsStartedProperty); }
            set { SetValue(IsStartedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDisplayTime. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsStartedProperty =
            DependencyProperty.Register("IsStarted", typeof(bool), typeof(TimerTextBlock), new UIPropertyMetadata(false));

        /// <summary>
        /// Format string for displaying the time span value.
        /// </summary>
        public string TimeFormat
        {
            get { return (string)GetValue(TimeFormatProperty); }
            set { SetValue(TimeFormatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimeFormat. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeFormatProperty =
            DependencyProperty.Register("TimeFormat", typeof(string), typeof(TimerTextBlock), new UIPropertyMetadata(null));

      

        // Using a DependencyProperty as the backing store for IsCountDown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCountDownProperty =
            DependencyProperty.Register("IsCountDown", typeof(bool), typeof(TimerTextBlock), new UIPropertyMetadata(false));

        /// <summary>
        /// Sets the time span to zero and stops the timer.
        /// </summary>
        public void Reset()
        {
            IsStarted = false;
            TimeSpan = TimeSpan.Zero;
        }

        private static void UpdateTimer(object state)
        {
            OnTick?.Invoke(null, EventArgs.Empty);
        }

        void TimerTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            Binding binding = new Binding("TimeSpan")
            {
                Source = this,
                Mode = BindingMode.OneWay,
                StringFormat = TimeFormat
            };

            SetBinding(TextProperty, binding);

            _updateTimeInvoker = UpdateTime;

            OnTick += TimerTextBlock_OnTick;
        }

        void TimerTextBlock_Unloaded(object sender, RoutedEventArgs e)
        {
            OnTick -= TimerTextBlock_OnTick;
        }

        void TimerTextBlock_OnTick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(_updateTimeInvoker);
        }

        private void UpdateTime()
        {
            TimeSpan step = TimeSpan.FromSeconds(1);
            if (IsStarted)
            {
                    TimeSpan += step;
            }
        }

        private void NotifyCountDownComplete()
        {
            OnCountDownComplete?.Invoke(this, EventArgs.Empty);
        }
    }
}
