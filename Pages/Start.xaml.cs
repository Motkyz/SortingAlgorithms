using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SortingAlgorithms.Pages
{
    public partial class Start : Page
    {
        public Start()
        {
            InitializeComponent();
        }

        private void InternalSort_Click(object sender, RoutedEventArgs e)
        {
            _ = NavigationService.Navigate(new Uri("/Pages/InternalSorting/InternalSorting.xaml", UriKind.Relative));
        }

        private void ExternalSort_Click(object sender, RoutedEventArgs e)
        {
            _ = NavigationService.Navigate(new Uri("/Pages/ExternalSorting/ExternalSorting.xaml", UriKind.Relative));
        }

        private void TextSort_Click(object sender, RoutedEventArgs e)
        {
            _ = NavigationService.Navigate(new Uri("/Pages/TextSorting/TextSorting.xaml", UriKind.Relative));
        }
    }
}
