namespace TaiwanStockTrading
{
    public class FieldsViewModel : ViewModelBase
    {
        private string _volume;
        private string _single;

        public string TotalVolume
        {
            get => _volume;
            set => SetProperty(ref _volume, value);
        }

        public string SingleChange
        {
            get => _single;
            set => SetProperty(ref _single, value);
        }
    }

}