using Microsoft.Web.WebView2.Wpf;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace GbyMail
{
    public class PdfViewerControl : UserControl
    {
        private WebView2? webView;
        private string? currentPdfPath;

        public static readonly DependencyProperty PdfPathProperty =
            DependencyProperty.Register("PdfPath", typeof(string), typeof(PdfViewerControl),
                new PropertyMetadata(null, OnPdfPathChanged));

        public string? PdfPath
        {
            get { return (string?)GetValue(PdfPathProperty); }
            set { SetValue(PdfPathProperty, value); }
        }

        public event Action<string>? LinkClicked;

        public PdfViewerControl()
        {
            Loaded += PdfViewerControl_Loaded;
            Unloaded += PdfViewerControl_Unloaded;
        }

        private async void PdfViewerControl_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeViewerAsync();
        }

        private void PdfViewerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                webView?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing WebView2: {ex.Message}");
            }
        }

        private async Task InitializeViewerAsync()
        {
            try
            {
                webView = new WebView2();
                webView.NavigationCompleted += WebView_NavigationCompleted;
                
                Content = webView;
                
                // Initialize WebView2
                await webView.EnsureCoreWebView2Async();
                
                // Setup event handlers for link detection
                webView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
                webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                
                // Load current PDF if available
                if (!string.IsNullOrEmpty(PdfPath))
                {
                    await LoadPdfAsync(PdfPath);
                }
            }
            catch (Exception ex)
            {
                // Fallback UI if WebView2 fails
                Content = CreateFallbackUI(ex.Message);
            }
        }

        private void CoreWebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            // Check if this is an https:// link click
            if (e.Uri.StartsWith("https://"))
            {
                // This is an https link click - handle it
                e.Cancel = true;
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LinkClicked?.Invoke(e.Uri);
                });
            }
            // Allow file:// URLs (for PDF loading) and about: URLs
            else if (!e.Uri.StartsWith("file:///") && !e.Uri.StartsWith("about:") && e.Uri.StartsWith("http"))
            {
                // Block http:// (non-secure) but still trigger email for any http links
                e.Cancel = true;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LinkClicked?.Invoke(e.Uri);
                });
            }
        }

        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var messageString = e.TryGetWebMessageAsString();
                if (!string.IsNullOrEmpty(messageString))
                {
                    var message = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(messageString);
                    if (message.TryGetProperty("type", out var typeProperty) && 
                        typeProperty.GetString() == "linkClick" &&
                        message.TryGetProperty("url", out var urlProperty))
                    {
                        var url = urlProperty.GetString();
                        if (!string.IsNullOrEmpty(url) && url.StartsWith("http"))
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                LinkClicked?.Invoke(url);
                            });
                        }
                    }
                }
            }
            catch
            {
                // Ignore parsing errors
            }
        }

        private void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // Inject JavaScript after navigation completes
            _ = InjectLinkDetectionScript();
        }

        private async Task InjectLinkDetectionScript()
        {
            try
            {
                if (webView?.CoreWebView2 != null)
                {
                    await webView.CoreWebView2.ExecuteScriptAsync(@"
                        (function() {
                            // Remove any existing listeners
                            if (window.gbyMailLinkHandler) {
                                document.removeEventListener('click', window.gbyMailLinkHandler);
                            }
                            
                            // Create new link handler for https:// links
                            window.gbyMailLinkHandler = function(e) {
                                var target = e.target;
                                
                                // Walk up the DOM tree to find an anchor tag
                                while (target && target.tagName !== 'A' && target.parentNode) {
                                    target = target.parentNode;
                                }
                                
                                // Check if we found a link and it's an https:// URL
                                if (target && target.tagName === 'A' && target.href) {
                                    var href = target.href;
                                    if (href.startsWith('https://') || href.startsWith('http://')) {
                                        e.preventDefault();
                                        e.stopPropagation();
                                        
                                        // Send message to C# code
                                        window.chrome.webview.postMessage({
                                            type: 'linkClick',
                                            url: href,
                                            timestamp: '2025-08-12 15:21:05',
                                            user: 'JMTDI'
                                        });
                                        
                                        console.log('GbyMail: Captured link click for', href);
                                        return false;
                                    }
                                }
                            };
                            
                            // Add event listener with capture = true to catch events early
                            document.addEventListener('click', window.gbyMailLinkHandler, true);
                            
                            // Also add mousedown for extra coverage
                            document.addEventListener('mousedown', function(e) {
                                if (e.target && e.target.tagName === 'A' && e.target.href) {
                                    var href = e.target.href;
                                    if (href.startsWith('https://') || href.startsWith('http://')) {
                                        console.log('GbyMail: Link detected on mousedown', href);
                                    }
                                }
                            }, true);
                            
                            console.log('GbyMail link detection initialized - 2025-08-12 15:21:05 UTC - User: JMTDI - Enhanced PDF Viewer Mode');
                        })();
                    ");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error injecting JavaScript: {ex.Message}");
            }
        }

        private static void OnPdfPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PdfViewerControl)d;
            _ = control.LoadPdfAsync((string?)e.NewValue);
        }

        private async Task LoadPdfAsync(string? path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    await LoadEmptyStateAsync();
                    return;
                }

                currentPdfPath = path;
                
                if (webView?.CoreWebView2 != null)
                {
                    // Load PDF directly using file:// URL for proper PDF rendering
                    var pdfUrl = $"file:///{path.Replace('\\', '/')}";
                    webView.CoreWebView2.Navigate(pdfUrl);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading PDF: {ex.Message}", "PDF Load Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                
                await LoadErrorStateAsync(ex.Message);
            }
        }

        private async Task LoadEmptyStateAsync()
        {
            var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>No PDF Selected - GbyMail</title>
    <meta charset='utf-8'>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
            background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
        }
        .container {
            text-align: center;
            padding: 50px;
            background: white;
            border-radius: 15px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.1);
            max-width: 500px;
        }
        .icon {
            font-size: 72px;
            margin-bottom: 20px;
        }
        .title {
            font-size: 24px;
            font-weight: bold;
            margin-bottom: 15px;
            color: #333;
        }
        .subtitle {
            font-size: 16px;
            color: #666;
            margin-bottom: 20px;
        }
        .feature {
            background: #e3f2fd;
            padding: 15px;
            border-radius: 8px;
            margin: 10px 0;
            font-size: 14px;
            color: #1976d2;
        }
        .timestamp {
            font-size: 10px;
            color: #999;
            margin-top: 20px;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='icon'>📄</div>
        <div class='title'>GbyMail PDF Viewer</div>
        <div class='subtitle'>Import a PDF file to view it with automatic https:// link detection</div>
        
        <div class='feature'>
            <strong>🔗 Auto-Email Feature:</strong><br>
            Click any https:// link in PDFs to automatically compose an email to auto@ibyfax.com
        </div>
        
        <div class='feature'>
            <strong>📧 No Body Mode:</strong><br>
            Emails will contain only the clicked link as the subject (no body content)
        </div>
        
        <div class='timestamp'>User: JMTDI | 2025-08-12 15:21:05 UTC</div>
    </div>
</body>
</html>";

            if (webView?.CoreWebView2 != null)
            {
                webView.CoreWebView2.NavigateToString(html);
            }
        }

        private async Task LoadErrorStateAsync(string error)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>PDF Load Error - GbyMail</title>
    <meta charset='utf-8'>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
            background: linear-gradient(135deg, #ffebee 0%, #ffcdd2 100%);
        }}
        .container {{
            text-align: center;
            padding: 40px;
            background: white;
            border-radius: 15px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.1);
            max-width: 500px;
        }}
        .error {{
            color: #d32f2f;
            font-size: 14px;
            margin-top: 20px;
            padding: 15px;
            background: #ffebee;
            border-radius: 8px;
            border-left: 4px solid #d32f2f;
        }}
        .timestamp {{
            font-size: 10px;
            color: #999;
            margin-top: 20px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>❌ PDF Load Error</h2>
        <p>Unable to load the PDF file in the viewer.</p>
        <div class='error'><strong>Error Details:</strong><br>{error}</div>
        <p>Please try importing a different PDF file or check if the file is corrupted.</p>
        <div class='timestamp'>User: JMTDI | 2025-08-12 15:21:05 UTC</div>
    </div>
</body>
</html>";

            if (webView?.CoreWebView2 != null)
            {
                webView.CoreWebView2.NavigateToString(html);
            }
        }

        private StackPanel CreateFallbackUI(string error)
        {
            return new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new TextBlock
                    {
                        Text = "📄",
                        FontSize = 64,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 20)
                    },
                    new TextBlock
                    {
                        Text = "PDF Viewer Unavailable",
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 15)
                    },
                    new TextBlock
                    {
                        Text = "WebView2 runtime is required for PDF viewing",
                        FontSize = 14,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 10),
                        Foreground = System.Windows.Media.Brushes.Gray
                    },
                    new TextBlock
                    {
                        Text = $"Technical Error: {error}",
                        FontSize = 11,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 20),
                        Foreground = System.Windows.Media.Brushes.Red,
                        TextWrapping = TextWrapping.Wrap,
                        MaxWidth = 400
                    },
                    new TextBlock
                    {
                        Text = "User: JMTDI | 2025-08-12 15:21:05 UTC",
                        FontSize = 8,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = System.Windows.Media.Brushes.Gray
                    }
                }
            };
        }

        private string GetFileSize(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                var bytes = fileInfo.Length;
                
                if (bytes < 1024)
                    return $"{bytes} B";
                else if (bytes < 1024 * 1024)
                    return $"{bytes / 1024.0:F1} KB";
                else if (bytes < 1024 * 1024 * 1024)
                    return $"{bytes / (1024.0 * 1024.0):F1} MB";
                else
                    return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}