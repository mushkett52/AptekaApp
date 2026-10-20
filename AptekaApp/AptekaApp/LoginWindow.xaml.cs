using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AptekaApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }


        private void AdminLoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordInput.Password;

            if (string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Введите логин и пароль администратора.",
                    "Авторизация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var user = App.con.kurs_Users
                .Where(u =>
                    u.login == login &&
                    u.password == password).FirstOrDefault();

            if (user == null)
            {
                MessageBox.Show(
                    "Неверный логин или пароль.",
                    "Ошибка входа",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                PasswordInput.Clear();
                return;
            }

            var role = App.con.kurs_Role
                .FirstOrDefault(r => r.id_role == user.id_role);

            if (role == null || role.name != "Администратор")
            {
                MessageBox.Show(
                    "У данной учётной записи нет прав администратора.",
                    "Доступ запрещён",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                PasswordInput.Clear();
                return;
            }

            var adminWindow = new AdminWindow(user);
            adminWindow.Show();
            this.Close();
        }


        private void GuestLoginButton_Click(object sender, RoutedEventArgs e)
        {
            var welcome = new WelcomeWindow();
            welcome.Show();
            this.Close();
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12)
            {
                var display = new DisplayWindow();
                display.Show();
            }
        }
    }
}