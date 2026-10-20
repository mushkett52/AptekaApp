using System;
using System.Windows;
using System.Windows.Threading;

namespace AptekaApp
{
    public partial class OrderSuccessWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private int _secondsLeft = 10;

        public bool ReturnToWelcome { get; private set; } = false;

        public OrderSuccessWindow(string orderNumber, bool hasPrescription)
        {
            InitializeComponent();

            OrderNumberText.Text = orderNumber;

            if (hasPrescription)
                PrescriptionNotice.Visibility = Visibility.Visible;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            UpdateTimerText();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _secondsLeft--;
            UpdateTimerText();

            if (_secondsLeft <= 0)
            {
                _timer.Stop();
                GoToWelcome();
            }
        }

        private void UpdateTimerText()
        {
            TimerText.Text = $"Автоматический возврат через {_secondsLeft} сек...";
        }

        private void NextOrder_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            ReturnToWelcome = false;
            Close();
        }

        private void BackToWelcome_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            GoToWelcome();
        }

        private void GoToWelcome()
        {
            ReturnToWelcome = true;
            Close();
        }
    }
}