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
        }

        private void AddFeedCommand(object sender, RoutedEventArgs e)
        {
            try
            {
                App.Manager.AddFeed(EditFeedUrl.Text);
                MessageBox.Show("Feed added succesfully.", "Sucess", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
