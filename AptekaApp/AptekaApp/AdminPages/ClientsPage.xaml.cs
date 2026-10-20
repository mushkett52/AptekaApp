using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AptekaApp
{
    public partial class ClientsPage : Page
    {
        private kurs_Client _selected;

        public ClientsPage()
        {
            InitializeComponent();
            LoadClients();
        }

        private void LoadClients(string filter = null)
        {
            var query = App.con.kurs_Client.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                string f = filter.Trim().ToLower();
                query = query.Where(c =>
                    c.LastName.ToLower().Contains(f) ||
                    c.FirstName.ToLower().Contains(f) ||
                    c.Telephone.ToLower().Contains(f));
            }

            ClientsGrid.ItemsSource = query
                .OrderBy(c => c.LastName)
                .ToList();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
            => LoadClients(SearchBox.Text);

        private void ClientsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selected = ClientsGrid.SelectedItem as kurs_Client;
            if (_selected == null) return;

            LastNameTextBox.Text = _selected.LastName;
            FirstNameTextBox.Text = _selected.FirstName;
            PatronymicTextBox.Text = _selected.Patronymic;
            PhoneTextBox.Text = _selected.Telephone;
            EmailTextBox.Text = _selected.Email;
            AddressTextBox.Text = _selected.Adress;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields()) return;

            try
            {
                var clientRole = App.con.kurs_Role
                    .FirstOrDefault(r => r.name == "Клиент");

                if (clientRole == null)
                {
                    MessageBox.Show(
                        "В системе отсутствует роль «Клиент».\n" +
                        "Добавьте её в таблицу kurs_Role.",
                        "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Генерируем логин на основе телефон + случайное число
                string baseLogin = "client_" + PhoneTextBox.Text.Replace("+", "").Replace("-", "").Trim();
                string login = baseLogin;
                int suffix = 1;
                while (App.con.kurs_Users.Any(u => u.login == login))
                {
                    login = baseLogin + "_" + suffix;
                    suffix++;
                }

                var newUser = new kurs_Users
                {
                    login = login,
                    password = "12345",
                    id_role = clientRole.id_role
                };
                App.con.kurs_Users.Add(newUser);
                App.con.SaveChanges();

                App.con.kurs_Client.Add(new kurs_Client
                {
                    id_user = newUser.id_user,
                    LastName = LastNameTextBox.Text.Trim(),
                    FirstName = FirstNameTextBox.Text.Trim(),
                    Patronymic = PatronymicTextBox.Text.Trim(),
                    Telephone = PhoneTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    Adress = AddressTextBox.Text.Trim()
                });
                App.con.SaveChanges();

                MessageBox.Show(
                    $"Клиент добавлен.\nЛогин: {login}\nПароль: 12345",
                    "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LoadClients(SearchBox.Text);
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
                MessageBox.Show("Выберите клиента для изменения.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidateFields()) return;

            try
            {
                _selected.LastName = LastNameTextBox.Text.Trim();
                _selected.FirstName = FirstNameTextBox.Text.Trim();
                _selected.Patronymic = PatronymicTextBox.Text.Trim();
                _selected.Telephone = PhoneTextBox.Text.Trim();
                _selected.Email = EmailTextBox.Text.Trim();
                _selected.Adress = AddressTextBox.Text.Trim();

                App.con.SaveChanges();
                LoadClients(SearchBox.Text);
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
                MessageBox.Show("Выберите клиента для удаления.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Удалить клиента «{_selected.LastName} {_selected.FirstName}»?\n" +
                "Вместе с ним будет удалена и учётная запись.",
                "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                int? userId = _selected.id_user;

                App.con.kurs_Client.Remove(_selected);
                App.con.SaveChanges();

                if (userId.HasValue)
                {
                    var user = App.con.kurs_Users
                        .FirstOrDefault(u => u.id_user == userId.Value);
                    if (user != null)
                    {
                        App.con.kurs_Users.Remove(user);
                        App.con.SaveChanges();
                    }
                }

                LoadClients(SearchBox.Text);
                ClearButton_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось удалить. Возможно, у клиента есть заказы.\n\n" + ex.Message,
                    "Ошибка удаления",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _selected = null;
            ClientsGrid.SelectedItem = null;

            LastNameTextBox.Clear();
            FirstNameTextBox.Clear();
            PatronymicTextBox.Clear();
            PhoneTextBox.Clear();
            EmailTextBox.Clear();
            AddressTextBox.Clear();
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                MessageBox.Show("Введите фамилию и имя клиента.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                MessageBox.Show("Введите телефон клиента.",
                                "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}