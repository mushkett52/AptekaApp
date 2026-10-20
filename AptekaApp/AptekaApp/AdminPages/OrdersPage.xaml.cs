using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AptekaApp
{
    public partial class OrdersPage : Page
    {
        private kurs_Order _selected;
        private List<OrderItem> _currentComposition = new List<OrderItem>();

        public OrdersPage()
        {
            InitializeComponent();
            OrderDatePicker.SelectedDate = DateTime.Today;
            LoadComboBoxes();
            LoadOrders();
        }

        private void LoadComboBoxes()
        {
            ClientComboBox.ItemsSource = App.con.kurs_Client
                .OrderBy(c => c.LastName).ToList();

            StatusComboBox.ItemsSource = App.con.kurs_Status
                .OrderBy(s => s.name).ToList();
        }

        private void LoadOrders(string filter = null)
        {
            var query = App.con.kurs_Order.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                string f = filter.Trim().ToLower();
                query = query.Where(o =>
                    o.kurs_Client.LastName.ToLower().Contains(f) ||
                    o.kurs_Status.name.ToLower().Contains(f) ||
                    o.orderNumber.ToLower().Contains(f));
            }

            OrdersGrid.ItemsSource = query
                .OrderByDescending(o => o.id_order)
                .ToList();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
            => LoadOrders(SearchBox.Text);

        private void OrdersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selected = OrdersGrid.SelectedItem as kurs_Order;
            if (_selected == null) return;

            ClientComboBox.SelectedValue = _selected.id_client;
            OrderDatePicker.SelectedDate = _selected.orderDate;
            StatusComboBox.SelectedValue = _selected.id_status;
            SumTextBox.Text = _selected.sum.ToString("0.00");

            _currentComposition = App.con.kurs_OrderSostav
                .Where(s => s.id_order == _selected.id_order)
                .Select(s => new OrderItem
                {
                    Id_Medicine = s.id_medicine,
                    Name = s.kurs_Medicine.name,
                    Price = s.kurs_Medicine.price,
                    Amount = s.amount
                })
                .ToList();
        }

        private void EditComposition_Click(object sender, RoutedEventArgs e)
        {
            var window = new OrderCompositionWindow(_currentComposition);
            window.Owner = Window.GetWindow(this);

            if (window.ShowDialog() == true)
            {
                _currentComposition = window.Result.ToList();
                SumTextBox.Text = _currentComposition.Sum(i => i.Total).ToString("0.00");
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields()) return;

            if (_currentComposition == null || _currentComposition.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одну позицию в состав заказа.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int nextId = App.con.kurs_Order.Any()
                    ? App.con.kurs_Order.Max(o => o.id_order) + 1
                    : 1;
                string orderNumber = $"ORD-{nextId:D4}";

                decimal sum = _currentComposition.Sum(i => i.Total);

                var newOrder = new kurs_Order
                {
                    id_client = ClientComboBox.SelectedValue as int?,
                    orderDate = OrderDatePicker.SelectedDate ?? DateTime.Today,
                    sum = sum,
                    id_status = (int)StatusComboBox.SelectedValue,
                    orderNumber = orderNumber
                };
                App.con.kurs_Order.Add(newOrder);
                App.con.SaveChanges();

                foreach (var item in _currentComposition)
                {
                    App.con.kurs_OrderSostav.Add(new kurs_OrderSostav
                    {
                        id_order = newOrder.id_order,
                        id_medicine = item.Id_Medicine,
                        amount = item.Amount
                    });
                }
                App.con.SaveChanges();

                LoadOrders(SearchBox.Text);
                ClearButton_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления:\n" + ex.Message,
                                "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null)
            {
                MessageBox.Show("Выберите заказ для изменения.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidateFields()) return;

            if (_currentComposition == null || _currentComposition.Count == 0)
            {
                MessageBox.Show("Состав заказа не может быть пустым.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _selected.id_client = ClientComboBox.SelectedValue as int?;
                _selected.orderDate = OrderDatePicker.SelectedDate ?? DateTime.Today;
                _selected.id_status = (int)StatusComboBox.SelectedValue;
                _selected.sum = _currentComposition.Sum(i => i.Total);

                var oldItems = App.con.kurs_OrderSostav
                    .Where(s => s.id_order == _selected.id_order)
                    .ToList();
                foreach (var item in oldItems)
                    App.con.kurs_OrderSostav.Remove(item);

                foreach (var item in _currentComposition)
                {
                    App.con.kurs_OrderSostav.Add(new kurs_OrderSostav
                    {
                        id_order = _selected.id_order,
                        id_medicine = item.Id_Medicine,
                        amount = item.Amount
                    });
                }

                App.con.SaveChanges();
                LoadOrders(SearchBox.Text);
                ClearButton_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка изменения:\n" + ex.Message,
                                "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null)
            {
                MessageBox.Show("Выберите заказ для удаления.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Удалить заказ №{_selected.orderNumber}?",
                "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var positions = App.con.kurs_OrderSostav
                    .Where(p => p.id_order == _selected.id_order)
                    .ToList();
                foreach (var p in positions)
                    App.con.kurs_OrderSostav.Remove(p);

                App.con.SaveChanges();

                App.con.kurs_Order.Remove(_selected);
                App.con.SaveChanges();

                LoadOrders(SearchBox.Text);
                ClearButton_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления:\n" + ex.Message,
                                "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _selected = null;
            _currentComposition = new List<OrderItem>();

            OrdersGrid.SelectedItem = null;
            ClientComboBox.SelectedIndex = -1;
            StatusComboBox.SelectedIndex = -1;
            OrderDatePicker.SelectedDate = DateTime.Today;
            SumTextBox.Clear();
        }

        private bool ValidateFields()
        {
            if (StatusComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите статус заказа.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (OrderDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Укажите дату заказа.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}