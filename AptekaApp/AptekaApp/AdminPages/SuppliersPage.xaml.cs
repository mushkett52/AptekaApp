using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AptekaApp
{
    public partial class SuppliersPage : Page
    {
        private kurs_Supplier _selected;

        public SuppliersPage()
        {
            InitializeComponent();
            LoadSuppliers();
        }

        private void LoadSuppliers(string filter = null)
        {
            var query = App.con.kurs_Supplier.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                string f = filter.Trim().ToLower();
                query = query.Where(s =>
                    s.name.ToLower().Contains(f) ||
                    s.ContactPerson.ToLower().Contains(f));
            }

            SuppliersGrid.ItemsSource = query
                .OrderBy(s => s.name)
                .ToList();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
            => LoadSuppliers(SearchBox.Text);

        private void SuppliersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selected = SuppliersGrid.SelectedItem as kurs_Supplier;
            if (_selected == null) return;

            NameTextBox.Text = _selected.name;
            PhoneTextBox.Text = _selected.Telephone;
            ContactPersonTextBox.Text = _selected.ContactPerson;
            EmailTextBox.Text = _selected.email;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields()) return;

            try
            {
                App.con.kurs_Supplier.Add(new kurs_Supplier
                {
                    name = NameTextBox.Text.Trim(),
                    Telephone = PhoneTextBox.Text.Trim(),
                    ContactPerson = ContactPersonTextBox.Text.Trim(),
                    email = EmailTextBox.Text.Trim()
                });
                App.con.SaveChanges();
                LoadSuppliers(SearchBox.Text);
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
                MessageBox.Show("Выберите поставщика для изменения.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidateFields()) return;

            try
            {
                _selected.name = NameTextBox.Text.Trim();
                _selected.Telephone = PhoneTextBox.Text.Trim();
                _selected.ContactPerson = ContactPersonTextBox.Text.Trim();
                _selected.email = EmailTextBox.Text.Trim();

                App.con.SaveChanges();
                LoadSuppliers(SearchBox.Text);
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
                MessageBox.Show("Выберите поставщика для удаления.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Удалить поставщика «{_selected.name}»?",
                "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                App.con.kurs_Supplier.Remove(_selected);
                App.con.SaveChanges();
                LoadSuppliers(SearchBox.Text);
                ClearButton_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось удалить. Возможно, у поставщика есть связанные лекарства.\n\n" + ex.Message,
                    "Ошибка удаления",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _selected = null;
            SuppliersGrid.SelectedItem = null;

            NameTextBox.Clear();
            PhoneTextBox.Clear();
            ContactPersonTextBox.Clear();
            EmailTextBox.Clear();
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите название поставщика.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                MessageBox.Show("Введите телефон поставщика.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}