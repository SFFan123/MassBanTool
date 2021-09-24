using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MassBanToolWPF.Annotations;

namespace MassBanToolWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        #region members

        private int _cooldown = 500;

        #endregion

        #region properties

        public int Cooldown
        {
            get => _cooldown;
            set
            {
                if (value < 301)
                {
                    throw new ArgumentException("Cooldown must be larger than 301ms.");
                }
                _cooldown = value;
                OnPropertyChanged(nameof(Cooldown));
            }
        }

        #endregion


        #region methods
        private void BtnApplyCooldown_Click(object sender, RoutedEventArgs e)
        {
            // TODO
            Console.WriteLine(Cooldown);
        }
        



        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        
    }
}
