using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AptekaApp
{
    public partial class ClientWindow : Window
    {
        private kurs_Client _currentClient;
        private readonly ObservableCollection<CartItem> _cart = new ObservableCollection<CartItem>();
        private List<kurs_Medicine> _allMedicines;

        public ClientWindow()
        {
            InitializeComponent();
            _currentClient = null;
            UserGreetingText.Text = "Вы вошли как: Аноним";
            Init();
        }

        public ClientWindow(kurs_Client client)
        {
            InitializeComponent();
            _currentClient = client;
            UserGreetingText.Text = _currentClient != null
                ? $"Вы вошли как: {client.LastName} {client.FirstName}"
                : "Вы вошли как: Аноним";
            Init();
        }

        private void Init()
        {
            CartList.ItemsSource = _cart;
            _cart.CollectionChanged += (s, e) => UpdateTotal();

            LoadCategories();
            LoadMedicines();
        }

        private void LoadCategories()
        {
            var categories = App.con.kurs_Category
                .OrderBy(c => c.name)
                .ToList();

            categories.Insert(0, new kurs_Category { id_category = 0, name = "Все категории" });

            CategoryFilterComboBox.ItemsSource = categories;
            CategoryFilterComboBox.SelectedIndex = 0;
        }

        private void LoadMedicines()
        {
            _allMedicines = App.con.kurs_Medicine
                .Where(m => m.amount > 0)
                .OrderBy(m => m.name)
                .ToList();

            ApplyFilters();
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allMedicines == null) return;

            var query = _allMedicines.AsEnumerable();

            string search = SearchBox.Text?.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m =>
                    m.name.ToLower().Contains(search) ||
                    m.manufacturer.ToLower().Contains(search));

            if (CategoryFilterComboBox.SelectedValue is int catId && catId != 0)
                query = query.Where(m => m.id_category == catId);

            if (decimal.TryParse(PriceFromBox.Text, out decimal from))
                query = query.Where(m => m.price >= from);

            if (decimal.TryParse(PriceToBox.Text, out decimal to))
                query = query.Where(m => m.price <= to);

            MedicinesList.ItemsSource = query.ToList();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Clear();
            PriceFromBox.Clear();
            PriceToBox.Clear();
            CategoryFilterComboBox.SelectedIndex = 0;
            ApplyFilters();
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var medicine = button?.Tag as kurs_Medicine;
            if (medicine == null) return;

            var existing = _cart.FirstOrDefault(c => c.Id_Medicine == medicine.id_Medicine);
            if (existing != null)
            {
                if (existing.Amount >= medicine.amount)
                {
                    MessageBox.Show(
                        $"В наличии только {medicine.amount} шт. товара «{medicine.name}».",
                        "Ограничение",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                existing.Amount++;
            }
            else
            {
                _cart.Add(new CartItem
                {
                    Id_Medicine = medicine.id_Medicine,
                    Name = medicine.name,
                    Price = medicine.price,
                    Amount = 1,
                    IsPrescription = medicine.prescription
                });
            }
        }

        private void IncreaseQty_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.Tag as CartItem;
            if (item == null) return;

            var med = _allMedicines.FirstOrDefault(m => m.id_Medicine == item.Id_Medicine);
            if (med != null && item.Amount >= med.amount)
            {
                MessageBox.Show(
                    $"В наличии только {med.amount} шт.",
                    "Ограничение",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            item.Amount++;
        }

        private void DecreaseQty_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.Tag as CartItem;
            if (item == null) return;

            if (item.Amount <= 1)
                _cart.Remove(item);
            else
                item.Amount--;
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.Tag as CartItem;
            if (item == null) return;

            _cart.Remove(item);
        }

        private void ClearCart_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count == 0) return;

            if (MessageBox.Show("Очистить корзину?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _cart.Clear();
            }
        }

        private void UpdateTotal()
        {
            decimal total = _cart.Sum(c => c.Total);
            TotalText.Text = $"{total:N2} ₽";

            PrescriptionWarning.Visibility = _cart.Any(c => c.IsPrescription)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("Корзина пуста. Добавьте товары.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                foreach (var item in _cart)
                {
                    var med = App.con.kurs_Medicine.First(m => m.id_Medicine == item.Id_Medicine);
                    if (med.amount < item.Amount)
                    {
                        MessageBox.Show(
                            $"Недостаточно товара «{med.name}» на складе " +
                            $"(нужно {item.Amount}, есть {med.amount}).",
                            "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                int nextId = App.con.kurs_Order.Any()
                    ? App.con.kurs_Order.Max(o => o.id_order) + 1
                    : 1;
                string orderNumber = $"ORD-{nextId:D4}";

                var status = App.con.kurs_Status
    .FirstOrDefault(s => s.name == "В обработке");
                if (status == null)
                {
                    MessageBox.Show("В системе нет статуса «Принят к сборке».",
                                    "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                decimal total = _cart.Sum(c => c.Total);
                bool hasPrescription = _cart.Any(c => c.IsPrescription);

                var order = new kurs_Order
                {
                    id_client = _currentClient?.id_client,
                    orderDate = DateTime.Today,
                    sum = total,
                    id_status = status.id_status,
                    orderNumber = orderNumber
                };
                App.con.kurs_Order.Add(order);
                App.con.SaveChanges();

                foreach (var item in _cart)
                {
                    App.con.kurs_OrderSostav.Add(new kurs_OrderSostav
                    {
                        id_order = order.id_order,
                        id_medicine = item.Id_Medicine,
                        amount = item.Amount
                    });

                    var med = App.con.kurs_Medicine.First(m => m.id_Medicine == item.Id_Medicine);
                    med.amount -= item.Amount;
                }
                App.con.SaveChanges();

                _cart.Clear();
                UpdateTotal();
                LoadMedicines();

                var successWindow = new OrderSuccessWindow(orderNumber, hasPrescription);
                successWindow.Owner = this;
                successWindow.ShowDialog();

                if (successWindow.ReturnToWelcome)
                {
                    var welcome = new WelcomeWindow();
                    welcome.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка оформления заказа:\n" + ex.Message,
                                "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadMedicines();
        }

        
    }
}