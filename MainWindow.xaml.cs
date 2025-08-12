using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GbyMail
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Set window title with current date
            Title = $"GbyMail - Search & Email Results (v2.3 - 2025-08-12 19:02:22 UTC)";
        }

        private void SearchComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    // Get the current text from the ComboBox
                    var comboBox = sender as ComboBox;
                    if (comboBox != null)
                    {
                        viewModel.SearchText = comboBox.Text;
                        viewModel.GoogleSearchCommand.Execute(null);
                    }
                }
            }
        }

        private void PdfViewerControl_LinkClicked(string link)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null)
            {
                // Show a brief notification that the https:// link was clicked
                var notification = new Window
                {
                    Title = "HTTPS Link Clicked - GbyMail",
                    Width = 400,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    ResizeMode = ResizeMode.NoResize,
                    Content = new StackPanel
                    {
                        Margin = new Thickness(20),
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "🔗 HTTPS Link Detected!",
                                FontSize = 18,
                                FontWeight = FontWeights.Bold,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Margin = new Thickness(0, 10, 0, 15),
                                Foreground = System.Windows.Media.Brushes.Green
                            },
                            new TextBlock
                            {
                                Text = "Opening email composer for auto@ibyfax.com...",
                                FontSize = 12,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Margin = new Thickness(0, 0, 0, 10)
                            },
                            new TextBlock
                            {
                                Text = $"Subject: {link}",
                                FontSize = 10,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                TextWrapping = TextWrapping.Wrap,
                                Foreground = System.Windows.Media.Brushes.Blue,
                                Margin = new Thickness(0, 0, 0, 10),
                                MaxWidth = 350
                            },
                            new TextBlock
                            {
                                Text = "Body: (Empty - No Body Mode)",
                                FontSize = 10,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Foreground = System.Windows.Media.Brushes.DarkGreen,
                                FontStyle = FontStyles.Italic,
                                Margin = new Thickness(0, 0, 0, 10)
                            },
                            new TextBlock
                            {
                                Text = "Timestamp: 2025-08-12 19:02:22 UTC | User: JMTDI",
                                FontSize = 8,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Foreground = System.Windows.Media.Brushes.Gray
                            }
                        }
                    }
                };

                notification.Show();

                // Auto-close notification after 5 seconds
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(5)
                };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    notification.Close();
                };
                timer.Start();

                // Handle the link click
                viewModel.HandlePdfLinkClick(link);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Clean up resources when window closes
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.Cleanup();
            }
            base.OnClosed(e);
        }
    }
}