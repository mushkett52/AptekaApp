using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AptekaApp
{
    public partial class CategoriesPage : Page
    {
        private kurs_Category _selected;

        public CategoriesPage()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            CategoriesGrid.ItemsSource = App.con.kurs_Category
                .OrderBy(c => c.name).ToList();
        }

        private void CategoriesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selected = CategoriesGrid.SelectedItem as kurs_Category;
            if (_selected == null) return;

            NameTextBox.Text = _selected.name;
            DescriptionTextBox.Text = _selected.description;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите название категории.");
                return;
            }

            try
            {
                App.con.kurs_Category.Add(new kurs_Category
                {
                    name = NameTextBox.Text.Trim(),
                    description = DescriptionTextBox.Text.Trim()
                });
                App.con.SaveChanges();
                LoadCategories();
                ClearButton_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null)
            {
                MessageBox.Show("Выберите категорию.");
                return;
            }
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите название.");
                return;
            }

            try
            {
                _selected.name = NameTextBox.Text.Trim();
                _selected.description = DescriptionTextBox.Text.Trim();
                App.con.SaveChanges();
                LoadCategories();
                ClearButton_Click(null, null);
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null)
            {
                MessageBox.Show("Выберите категорию.");
                return;
            }

            if (MessageBox.Show($"Удалить «{_selected.name}»?",
                                "Подтверждение",
                                MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            try
            {
                App.con.kurs_Category.Remove(_selected);
                App.con.SaveChanges();
                LoadCategories();
                ClearButton_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Нельзя удалить категорию, есть связанные лекарства.\n" + ex.Message);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _selected = null;
            CategoriesGrid.SelectedItem = null;
            NameTextBox.Clear();
            DescriptionTextBox.Clear();
        }
    }
}