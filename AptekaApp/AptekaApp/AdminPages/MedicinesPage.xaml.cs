using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AptekaApp
{
    public partial class MedicinesPage : Page
    {
        private kurs_Medicine _selected;

        public MedicinesPage()
        {
            InitializeComponent();
            LoadComboBoxes();
            LoadMedicines();
        }

        private void LoadComboBoxes()
        {
            CategoryComboBox.ItemsSource = App.con.kurs_Category
                .OrderBy(c => c.name).ToList();
            SupplierComboBox.ItemsSource = App.con.kurs_Supplier
                .OrderBy(s => s.name).ToList();
        }

        private void LoadMedicines(string filter = null)
        {
            var query = App.con.kurs_Medicine.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                string f = filter.Trim().ToLower();
                query = query.Where(m =>
                    m.name.ToLower().Contains(f) ||
                    m.manufacturer.ToLower().Contains(f) ||
                    m.kurs_Category.name.ToLower().Contains(f));
            }

            MedicinesGrid.ItemsSource = query
                .OrderBy(m => m.name)
                .ToList();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
            => LoadMedicines(SearchBox.Text);

        private void MedicinesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selected = MedicinesGrid.SelectedItem as kurs_Medicine;
            if (_selected == null) return;

            NameTextBox.Text = _selected.name;
            ManufacturerTextBox.Text = _selected.manufacturer;
            PriceTextBox.Text = _selected.price.ToString("0.00");
            AmountTextBox.Text = _selected.amount.ToString();
            MinAmountTextBox.Text = _selected.minAmount?.ToString() ?? "";
            ExpDatePicker.SelectedDate = _selected.expDate;
            PrescriptionCheckBox.IsChecked = _selected.prescription;
            ImageUrlTextBox.Text = _selected.imageUrl;

            CategoryComboBox.SelectedValue = _selected.id_category;
            SupplierComboBox.SelectedValue = _selected.id_supplier;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields(out decimal price, out int amount, out int? minAmount))
                return;

            try
            {
                var newMed = new kurs_Medicine
                {
                    name = NameTextBox.Text.Trim(),
                    manufacturer = ManufacturerTextBox.Text.Trim(),
                    price = price,
                    amount = amount,
                    minAmount = minAmount,
                    expDate = ExpDatePicker.SelectedDate,
                    prescription = PrescriptionCheckBox.IsChecked == true,
                    id_category = (int)CategoryComboBox.SelectedValue,
                    id_supplier = (int)SupplierComboBox.SelectedValue,
                    imageUrl = ImageUrlTextBox.Text.Trim()
                };

                App.con.kurs_Medicine.Add(newMed);
                App.con.SaveChanges();

                LoadMedicines(SearchBox.Text);
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
                MessageBox.Show("Выберите лекарство для изменения.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateFields(out decimal price, out int amount, out int? minAmount))
                return;

            try
            {
                _selected.name = NameTextBox.Text.Trim();
                _selected.manufacturer = ManufacturerTextBox.Text.Trim();
                _selected.price = price;
                _selected.amount = amount;
                _selected.minAmount = minAmount;
                _selected.expDate = ExpDatePicker.SelectedDate;
                _selected.prescription = PrescriptionCheckBox.IsChecked == true;
                _selected.id_category = (int)CategoryComboBox.SelectedValue;
                _selected.id_supplier = (int)SupplierComboBox.SelectedValue;
                _selected.imageUrl = ImageUrlTextBox.Text.Trim();

                App.con.SaveChanges();
                LoadMedicines(SearchBox.Text);
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
                MessageBox.Show("Выберите лекарство для удаления.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Удалить «{_selected.name}»?",
                "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                App.con.kurs_Medicine.Remove(_selected);
                App.con.SaveChanges();
                LoadMedicines(SearchBox.Text);
                ClearButton_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось удалить. Возможно, лекарство используется в заказах.\n\n" + ex.Message,
                    "Ошибка удаления",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _selected = null;
            MedicinesGrid.SelectedItem = null;

            NameTextBox.Clear();
            ManufacturerTextBox.Clear();
            PriceTextBox.Clear();
            AmountTextBox.Clear();
            MinAmountTextBox.Clear();
            ExpDatePicker.SelectedDate = null;
            PrescriptionCheckBox.IsChecked = false;
            ImageUrlTextBox.Clear();
            CategoryComboBox.SelectedIndex = -1;
            SupplierComboBox.SelectedIndex = -1;
        }

        private bool ValidateFields(out decimal price, out int amount, out int? minAmount)
        {
            price = 0;
            amount = 0;
            minAmount = null;

            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                string.IsNullOrWhiteSpace(ManufacturerTextBox.Text))
            {
                MessageBox.Show("Заполните название и производителя.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CategoryComboBox.SelectedValue == null ||
                SupplierComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите категорию и поставщика.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(PriceTextBox.Text, out price) || price < 0)
            {
                MessageBox.Show("Цена должна быть неотрицательным числом.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(AmountTextBox.Text, out amount) || amount < 0)
            {
                MessageBox.Show("Количество должно быть неотрицательным целым числом.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(MinAmountTextBox.Text))
            {
                if (!int.TryParse(MinAmountTextBox.Text, out int min) || min < 0)
                {
                    MessageBox.Show("Мин. остаток должен быть неотрицательным числом.",
                                    "Внимание",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                minAmount = min;
            }

            return true;
        }
    }
}