using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia.Controls;
using DynamicData;
using MessageBox.Avalonia.Enums;
using ReactiveUI;

namespace MassBanToolMP.ViewModels
{
    public class EditIENumerableDialogViewModel : ViewModelBase
    {
        private ObservableCollection<CacheEntry> _objects;

        public EditIENumerableDialogViewModel()
        {
            _objects = new ObservableCollection<CacheEntry>();
        }
        public EditIENumerableDialogViewModel(string header, IEnumerable<string> Objects)
        {
            _objects = new ObservableCollection<CacheEntry>(Objects.Select(x => new CacheEntry { Value = x }));
            Header = header;
            OnDataGridRemoveEntry = ReactiveCommand.Create<object>(RemoveEntry);
            CancelWindowCloseCommand = ReactiveCommand.Create<Window>(CloseCancel);
            OKWindowCloseCommand = ReactiveCommand.Create<Window>(CloseOK);
        }

        private void RemoveEntry(object selectedItems)
        {
            if (selectedItems is IList item)
            {
                var items = new List<CacheEntry>();
                for (var i = 0; i < item.Count; i++)
                    if (item[i] is CacheEntry entry)
                        items.Add(entry);

                Objects.RemoveMany(items);
            }
        }

        public string Header { get; set; }

        public ReactiveCommand<object, Unit> OnDataGridRemoveEntry { get; } 
        public ReactiveCommand<Window, Unit> CancelWindowCloseCommand { get; } 
        public ReactiveCommand<Window, Unit> OKWindowCloseCommand { get; }

        public ObservableCollection<CacheEntry> Objects
        {
            get => _objects;
            set => SetProperty(ref _objects, value);
        }

        public bool Unique { get; set; }

        private void CloseOK(Window window)
        {
            window.Close(ButtonResult.Ok);
        }

        private void CloseCancel(Window window)
        {
            window.Close(ButtonResult.Cancel);
        }

        public class CacheEntry : ViewModelBase
        {
            private string _value;

            public string Value
            {
                get => _value;
                set => SetProperty(ref _value, value);
            }
        }
    }
}
