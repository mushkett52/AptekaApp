using System.Windows;
using System.Windows.Controls;

namespace AptekaApp
{
    public partial class AdminWindow : Window
    {
        private readonly kurs_Users _currentUser;

        public AdminWindow(kurs_Users user)
        {
            InitializeComponent();
            _currentUser = user;

            this.Title = $"Интернет-аптека — Администратор ({user.login})";
            UserInfoText.Text = $"Вы вошли как: {user.login}";

            MainFrame.Navigate(new MedicinesPage());
        }

        private void MenuMedicines_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new MedicinesPage());

        private void MenuCategories_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new CategoriesPage());

        private void MenuSuppliers_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new SuppliersPage());

        
        private void MenuOrders_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new OrdersPage());

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}