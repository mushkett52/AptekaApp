using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace AptekaApp
{
    public partial class DisplayWindow : Window
    {
        private readonly DispatcherTimer _refreshTimer;
        private readonly DispatcherTimer _clockTimer;

        public DisplayWindow()
        {
            InitializeComponent();

            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _refreshTimer.Tick += (s, e) => LoadOrders();
            _refreshTimer.Start();

            _clockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _clockTimer.Tick += (s, e) => UpdateClock();
            _clockTimer.Start();

            LoadOrders();
            UpdateClock();
        }

        private void LoadOrders()
        {
            try
            {
                var orders = App.con.kurs_Order
                    .OrderBy(o => o.id_order)
                    .Select(o => new
                    {
                        OrderNumber = o.orderNumber,
                        StatusName = o.kurs_Status.name
                    })
                    .ToList();

                var inProgress = orders
    .Where(o => o.StatusName == "В обработке" || o.StatusName == "Собирается")
    .ToList();

                var ready = orders
                    .Where(o => o.StatusName == "Готов к выдаче")
                    .ToList();

                InProgressList.ItemsSource = inProgress;
                ReadyList.ItemsSource = ready;

                UpdateTimeText.Text = $"Последнее обновление: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                UpdateTimeText.Text = "Ошибка загрузки: " + ex.Message;
            }
        }

        private void UpdateClock()
        {
            ClockText.Text = DateTime.Now.ToString("dd.MM.yyyy  HH:mm:ss");
        }
    }
}