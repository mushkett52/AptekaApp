using System.ComponentModel;

namespace AptekaApp
{
    public class OrderItem : INotifyPropertyChanged
    {
        private int _amount;

        public int Id_Medicine { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public int Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
                OnPropertyChanged(nameof(Total));
            }
        }

        public decimal Total => Price * Amount;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}