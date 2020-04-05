namespace AndroidExplorer
{
    class DetailItem
        : DataModel
    {
        private string _value;

        public DetailItem(string name, string value)
        {
            Name = name;
            _value = value;
        }

        public string Name { get; }

        public string Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (value != _value)
                {
                    _value = value;
                    RaisePropertyChanged("Value");
                }
            }
        }
    }
}
