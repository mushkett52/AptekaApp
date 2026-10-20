using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AptekaApp
{
    public partial class OrderCompositionWindow : Window
    {
        private readonly ObservableCollection<OrderItem> _composition =
            new ObservableCollection<OrderItem>();

        public IReadOnlyList<OrderItem> Result => _composition;

        public OrderCompositionWindow()
        {
            InitializeComponent();
            CompositionGrid.ItemsSource = _composition;
            _composition.CollectionChanged += (s, e) => UpdateTotal();

            LoadMedicines();
        }

        public OrderCompositionWindow(IEnumerable<OrderItem> existing) : this()
        {
            if (existing != null)
            {
                foreach (var item in existing)
                    _composition.Add(new OrderItem
                    {
                        Id_Medicine = item.Id_Medicine,
                        Name = item.Name,
                        Price = item.Price,
                        Amount = item.Amount
                    });
            }
        }

        private void LoadMedicines()
        {
            MedicineComboBox.ItemsSource = App.con.kurs_Medicine
                .OrderBy(m => m.name)
                .ToList();
            try
            {
                MedicineComboBox.ItemsSource = App.con.kurs_Medicine
                    .OrderBy(m => m.name)
                    .ToList();
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                ShowFullException(ex);
            }
            catch (System.Data.Entity.Core.EntityException ex)
            {
                ShowFullException(ex);
            }
        }
        private void ShowFullException(System.Exception ex)
        {
            var msg = ex.Message;
            var inner = ex.InnerException;
            while (inner != null)
            {
                msg += "\n\n--- INNER ---\n" + inner.Message;
                inner = inner.InnerException;
            }
            MessageBox.Show(msg, "EF Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void AddPosition_Click(object sender, RoutedEventArgs e)
        {
            var medicine = MedicineComboBox.SelectedItem as kurs_Medicine;
            if (medicine == null)
            {
                MessageBox.Show("Выберите лекарство.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(AmountBox.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Количество должно быть положительным целым числом.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existing = _composition.FirstOrDefault(c => c.Id_Medicine == medicine.id_Medicine);
            if (existing != null)
            {
                existing.Amount += qty;
            }
            else
            {
                _composition.Add(new OrderItem
                {
                    Id_Medicine = medicine.id_Medicine,
                    Name = medicine.name,
                    Price = medicine.price,
                    Amount = qty
                });
            }

            MedicineComboBox.SelectedIndex = -1;
            AmountBox.Text = "1";
        }

        private void RemoveRow_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.Tag as OrderItem;
            if (item == null) return;

            _composition.Remove(item);
        }

        private void UpdateTotal()
        {
            decimal total = _composition.Sum(i => i.Total);
            CompositionTotalText.Text = $"{total:N2} ₽";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_composition.Count == 0)
            {
                MessageBox.Show("Состав заказа пуст. Добавьте хотя бы одну позицию.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}