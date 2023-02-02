using System;
using System.Collections;
using System.ComponentModel;

namespace map_app.Models
{
    public class Tag : INotifyDataErrorInfo, INotifyPropertyChanged
    {
        private string _name;
        private bool _hasErrors = false;

        public Tag()
        {
            _name = "New";
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                _hasErrors = string.IsNullOrWhiteSpace(_name);
                OnErrorsChanged(nameof(Name));
                OnPropertyChanged(nameof(Name));
            }
        }

        public string? Value { get; set; }

        public bool HasErrors => _hasErrors;

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public IEnumerable GetErrors(string? propertyName)
        {
            return _hasErrors 
                ? new[] { "Имя метки не должно быть пустым" }
                : Array.Empty<object>();
        }

        private void OnErrorsChanged(string propertyName)
            => ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));       

        private void OnPropertyChanged(string? propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}