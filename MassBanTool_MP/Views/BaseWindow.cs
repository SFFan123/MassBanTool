﻿using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using MassBanToolMP.ViewModels;
using ReactiveUI;

namespace MassBanToolMP.Views
{
    public abstract class BaseWindow<T> : ReactiveWindow<T> where T : ViewModelBase
    {
        protected BaseWindow()
        {
            this.WhenActivated((CompositeDisposable disposable) =>
            {
                this.ViewModel.WhenAnyValue(x => x.IsBusy)
                    .Do(UpdateCursor)
                    .Subscribe()
                    .DisposeWith(disposable);
            });
        }

        private void UpdateCursor(bool show)
        {
            void update(bool _show)
            {
                Cursor = show ? new Cursor(StandardCursorType.Wait) : Cursor.Default;
            }

            if (Dispatcher.UIThread.CheckAccess())
            {
                update(show);
            }
            else
            {
                Dispatcher.UIThread.InvokeAsync(() => update(show));
            }
        }
    }
}