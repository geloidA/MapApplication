using System.Text.RegularExpressions;
using System.Windows.Input;
using Avalonia.Input;
using map_app.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace map_app.ViewModels
{
    public class SettingsViewModel : ReactiveValidationObject
    {
        private readonly Regex _validateIPv4Regex = new("^(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
        private readonly MainViewModel _mainViewModel;

        [Reactive]
        internal string? DeliveryIPAddress { get; set; }

        [Reactive]
        internal int DeliveryPort { get; set; }

        public SettingsViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            this.ValidationRule(x => x.DeliveryIPAddress,
                                ip => !string.IsNullOrEmpty(ip) && _validateIPv4Regex.IsMatch(ip),
                                "Неправильный IP-адрес");
            this.ValidationRule(x => x.DeliveryPort,
                                port => port > 1024 && port < 49151,
                                "Неправильный порт");
            DeliveryPort = mainViewModel.DeliveryPort;
            DeliveryIPAddress = mainViewModel.DeliveryIPAddress;
            Confirm = ReactiveCommand.Create<ICloseable>(ConfirmImpl, canExecute: this.IsValid());
            Close = ReactiveCommand.Create<ICloseable>(WindowCloser.Close);
        }

        private void ConfirmImpl(ICloseable wnd)
        {
            _mainViewModel.DeliveryIPAddress = DeliveryIPAddress;
            if (_mainViewModel.DeliveryPort != DeliveryPort)
                _mainViewModel.DeliveryPort = DeliveryPort;
            Close?.Execute(wnd);
        }

        internal ICommand? Close { get; private set; }
        internal ICommand? Confirm { get; private set; }
    }
}