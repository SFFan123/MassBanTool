using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MassBanToolMP.ViewModels
{
    public class ViewModelBase :INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private Dictionary<string, List<string>> _propertyErrors = new Dictionary<string, List<string>>();
        private bool isBusy;

        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;

            storage = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        public IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName == null)
                return _propertyErrors;

            if (_propertyErrors.ContainsKey(propertyName))
                return _propertyErrors[propertyName];

            return null;
        }

        public bool HasErrors
        {
            get => _propertyErrors.Count > 0;
        }
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        protected void AddError(string propertyName, string errorMessage)
        {
            if (!_propertyErrors.ContainsKey(propertyName))
            {
                _propertyErrors.Add(propertyName, new List<string>());
            }
            else if (_propertyErrors[propertyName].Contains(errorMessage))
            {
                return;
            }

            _propertyErrors[propertyName].Add(errorMessage);
            OnErrorsChanged(propertyName);
        }

        protected void ClearError(string propertyName, string errorMessage = null)
        {
            if (_propertyErrors.ContainsKey(propertyName))
            {
                if (string.IsNullOrEmpty(errorMessage))
                {
                    _propertyErrors[propertyName].Clear();
                }
                else
                {
                    _propertyErrors[propertyName].Remove(errorMessage);
                }
            }
        }

        protected void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }
    }
}
