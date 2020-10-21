using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Fidd
{
    /// <summary>
    /// Interaction logic for WindowEditFeed.xaml
    /// </summary>
    public partial class WindowEditFeed : Window
    {
        public Feed Feed { get; set; }
        string new_feed_image = null;

        public WindowEditFeed()
        {
            InitializeComponent();
            DataContext = this;
        }

        public WindowEditFeed(Feed feed) : this() {
            Feed = feed;
            if (Feed.ImagePath != null)
            {
                ImageFeedIcon.Source = App.LoadImageFile(Path.Combine(App.FeedManager.FeedDirectory, Feed.ImagePath));
                ButtonRemoveImage.IsEnabled = true;
            }
        }

        private async void DoUpdate(object sender, RoutedEventArgs e)
        {
            if (new_feed_image != null)
            {
                if (string.IsNullOrEmpty(new_feed_image))
                {
                    File.Delete(Path.Combine(App.FeedManager.FeedDirectory, Feed.ImagePath));
                    Feed.ImagePath = null;
                }
                else
                {
                    var ext = Path.GetExtension(new_feed_image);
                    Feed.ImagePath = $"{Feed.ID}{ext}";
                    try
                    {
                        File.Move(
                            Path.Combine(App.FeedManager.FeedDirectory, new_feed_image),
                            Path.Combine(App.FeedManager.FeedDirectory, Feed.ImagePath),
                            true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            await App.FeedManager.UpdateFeedMetadataAsync(Feed);
            Close();
        }

        private void ChangeFeedImage(object sender, RoutedEventArgs e)
        {
            var select_image_dialog = new OpenFileDialog()
            {
                Title = "Select an image...",
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Multiselect = false,
                Filter = "Image files|*.png;*.jpg;*.jpeg|All files|*.*"
            };
            if (select_image_dialog.ShowDialog() == true)
            {
                var ext = Path.GetExtension(select_image_dialog.FileName);
                new_feed_image = $"tmp_{Feed.ID}{ext}";
                var new_image_path = Path.Combine(App.FeedManager.FeedDirectory, new_feed_image);
                File.Copy(select_image_dialog.FileName, new_image_path);
                ImageFeedIcon.Source = App.LoadImageFile(new_image_path);
                ButtonRemoveImage.IsEnabled = true;
            }
        }

        private void RemoveFeedImage(object sender, RoutedEventArgs e)
        {
            new_feed_image = Feed.ImagePath is null? null : string.Empty;
            ImageFeedIcon.Source = new BitmapImage(new Uri("/Icons/no_image.png", UriKind.Relative));
            ButtonRemoveImage.IsEnabled = false;
        }
    }
}
