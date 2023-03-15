using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;

namespace map_app.Models
{
    public class UserTag : INotifyDataErrorInfo, INotifyPropertyChanged
    {
        private bool _hasErrors;
        private string _name = "New";

        public UserTag()
        {
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
            if (!HasErrors) return Enumerable.Empty<object>();
            if (propertyName != nameof(Name)) 
                throw new NotImplementedException();
            return new[] { "Имя метки не может быть пустым" };
        }

        private void OnErrorsChanged(string propertyName)
            => ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));       

        private void OnPropertyChanged(string? propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}