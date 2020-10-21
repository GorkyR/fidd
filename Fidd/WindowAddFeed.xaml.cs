using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Fidd
{
    /// <summary>
    /// Interaction logic for WindowAddFeed.xaml
    /// </summary>
    public partial class WindowAddFeed : Window
    {
        public WindowAddFeed()
        {
            InitializeComponent();
            EditFeedUrl.Focus();
        }

        private async void AddFeedCommand(object sender, RoutedEventArgs e)
        {
            EditFeedUrl.IsEnabled = ButtonAdd.IsEnabled = false;
            ProgressWorking.Visibility = Visibility.Visible;
            try
            {
                await App.FeedManager.AddFeedAsync(EditFeedUrl.Text);
                // Commented out cuz you shouldn't be bothered on success
                //MessageBox.Show("Feed added succesfully.", "Sucess", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex) { MessageBox.Show($"Error:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Information); }
            ProgressWorking.Visibility = Visibility.Hidden;
            EditFeedUrl.IsEnabled = ButtonAdd.IsEnabled = true;
        }
    }
}
