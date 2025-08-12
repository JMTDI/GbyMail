using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace GbyMail
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string searchText = "";

        [ObservableProperty]
        private string generatedURL = "";

        [ObservableProperty]
        private string lastSearchTime = "";

        [ObservableProperty]
        private int selectedTabIndex = 0;

        [ObservableProperty]
        private ObservableCollection<EmailItem> receivedEmails = new();

        [ObservableProperty]
        private EmailItem? selectedEmail;

        [ObservableProperty]
        private string? selectedPdfPath;

        [ObservableProperty]
        private ObservableCollection<string> searchHistory = new();

        public bool IsSearchEnabled => !string.IsNullOrWhiteSpace(SearchText);

        public bool HasSearchHistory => searchHistory.Count > 0;

        public Visibility LastEmailVisibility => string.IsNullOrEmpty(GeneratedURL) ? Visibility.Collapsed : Visibility.Visible;

        public Visibility EmptyInboxVisibility => ReceivedEmails.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility EmailListVisibility => ReceivedEmails.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility NoPdfSelectedVisibility => string.IsNullOrEmpty(SelectedPdfPath) ? Visibility.Visible : Visibility.Collapsed;

        public Visibility PdfViewerVisibility => !string.IsNullOrEmpty(SelectedPdfPath) ? Visibility.Visible : Visibility.Collapsed;

        public string SelectedPdfName => string.IsNullOrEmpty(SelectedPdfPath) ? "" : Path.GetFileName(SelectedPdfPath);

        private readonly string _gbyMailPath;
        private readonly string _searchHistoryPath;

        public MainViewModel()
        {
            // Create GbyMail directory if it doesn't exist
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _gbyMailPath = Path.Combine(documentsPath, "GbyMail");
            _searchHistoryPath = Path.Combine(_gbyMailPath, "search_history.txt");

            if (!Directory.Exists(_gbyMailPath))
            {
                Directory.CreateDirectory(_gbyMailPath);
            }

            LoadSearchHistory();
            CheckForSharedDocuments();
        }

        [RelayCommand]
        private void GoogleSearch()
        {
            PerformSearch(SearchPlatform.Google);
        }

        [RelayCommand]
        private void DuckDuckGoSearch()
        {
            PerformSearch(SearchPlatform.DuckDuckGo);
        }

        [RelayCommand]
        private void EbaySearch()
        {
            PerformSearch(SearchPlatform.Ebay);
        }

        [RelayCommand]
        private void AmazonSearch()
        {
            PerformSearch(SearchPlatform.Amazon);
        }

        [RelayCommand]
        private void ClearSearchHistory()
        {
            searchHistory.Clear();
            SaveSearchHistory();
            OnPropertyChanged(nameof(HasSearchHistory));
        }

        [RelayCommand]
        private void ImportPdf()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
                Title = "Select a PDF file to import into GbyMail",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                HandleSelectedPdf(openFileDialog.FileName);
            }
        }

        [RelayCommand]
        private void Refresh()
        {
            CheckForSharedDocuments();
        }

        [RelayCommand]
        private void GoToInbox()
        {
            SelectedTabIndex = 1;
        }

        [RelayCommand]
        private void ClosePdf()
        {
            SelectedPdfPath = null;
            OnPropertyChanged(nameof(NoPdfSelectedVisibility));
            OnPropertyChanged(nameof(PdfViewerVisibility));
        }

        [RelayCommand]
        private void SendPdfLink()
        {
            if (string.IsNullOrEmpty(SelectedPdfPath))
                return;

            var pdfFileName = Path.GetFileName(SelectedPdfPath);
            var subject = $"PDF File: {pdfFileName}";

            SendEmail(subject);
        }

        [RelayCommand]
        private void OpenPdfFolder()
        {
            if (string.IsNullOrEmpty(SelectedPdfPath) || !File.Exists(SelectedPdfPath))
                return;

            try
            {
                var folderPath = Path.GetDirectoryName(SelectedPdfPath);
                if (!string.IsNullOrEmpty(folderPath))
                {
                    Process.Start("explorer.exe", $"/select,\"{SelectedPdfPath}\"");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void HandlePdfLinkClick(string link)
        {
            var subject = link; // Use the clicked https:// link as the subject
            SendEmail(subject);
        }

        public void HandleSelectedPdf(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("The selected PDF file does not exist.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Copy PDF to GbyMail directory for persistence
                var fileName = Path.GetFileName(filePath);
                var destinationPath = Path.Combine(_gbyMailPath, fileName);

                // If file already exists, add timestamp to make it unique
                if (File.Exists(destinationPath))
                {
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    var extension = Path.GetExtension(fileName);
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    fileName = $"{nameWithoutExt}_{timestamp}{extension}";
                    destinationPath = Path.Combine(_gbyMailPath, fileName);
                }

                File.Copy(filePath, destinationPath, true);

                var email = new EmailItem
                {
                    Id = Guid.NewGuid(),
                    From = "Imported PDF",
                    Subject = fileName,
                    Body = $"PDF imported to GbyMail via file picker\nOriginal location: {filePath}\nFile size: {GetFileSize(destinationPath)}\nImported by: JMTDI\nTimestamp: 2025-08-12 19:02:22 UTC",
                    Date = DateTime.Now,
                    PdfAttachment = destinationPath
                };

                ReceivedEmails.Add(email);
                SelectedPdfPath = destinationPath;
                SelectedTabIndex = 2; // Switch to PDF viewer

                RefreshVisibilityProperties();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing PDF: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Cleanup()
        {
            SaveSearchHistory();
        }

        private void SendEmail(string subject)
        {
            // Create mailto URL with only subject - no body
            var mailtoUrl = $"mailto:auto@ibyfax.com?subject={Uri.EscapeDataString(subject)}";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = mailtoUrl,
                    UseShellExecute = true
                });

                // Update the generated URL and time for display
                GeneratedURL = subject;
                LastSearchTime = "2025-08-12 19:02:22 UTC";
                OnPropertyChanged(nameof(LastEmailVisibility));

                // Show confirmation
                MessageBox.Show($"Email composer opened with:\n\nTo: auto@ibyfax.com\nSubject: {subject}\nBody: (Empty)\n\nUser: JMTDI\nTimestamp: 2025-08-12 19:02:22 UTC",
                    "Email Composed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open mail client: {ex.Message}\n\nPlease ensure you have a default email client configured.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PerformSearch(SearchPlatform platform)
        {
            var trimmedText = SearchText?.Trim();
            if (string.IsNullOrEmpty(trimmedText))
            {
                MessageBox.Show("Please enter a search term", "GbyMail", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Add to search history
            AddToSearchHistory(trimmedText);

            GeneratedURL = CreateSearchURL(trimmedText, platform);
            LastSearchTime = "2025-08-12 19:02:22 UTC";

            SendEmail(GeneratedURL);

            SearchText = "";
            OnPropertyChanged(nameof(IsSearchEnabled));
        }

        private string CreateSearchURL(string searchText, SearchPlatform platform)
        {
            var searchWithPlus = searchText.Replace(" ", "+");
            var encodedSearchText = Uri.EscapeDataString(searchWithPlus).Replace("%2B", "+");

            return platform switch
            {
                SearchPlatform.Google => $"https://www.google.com/search?q={encodedSearchText}",
                SearchPlatform.DuckDuckGo => $"https://duckduckgo.com/?q={encodedSearchText}",
                SearchPlatform.Ebay => $"https://www.ebay.com/sch/i.html?_nkw={encodedSearchText}",
                SearchPlatform.Amazon => $"https://www.amazon.com/s?k={encodedSearchText}",
                _ => ""
            };
        }

        private void AddToSearchHistory(string searchTerm)
        {
            // Remove if already exists (to move to top)
            var existingItem = searchHistory.FirstOrDefault(h => h.Equals(searchTerm, StringComparison.OrdinalIgnoreCase));
            if (existingItem != null)
            {
                searchHistory.Remove(existingItem);
            }

            // Add to beginning of list
            searchHistory.Insert(0, searchTerm);

            // Limit to 20 items
            while (searchHistory.Count > 20)
            {
                searchHistory.RemoveAt(searchHistory.Count - 1);
            }

            SaveSearchHistory();
            OnPropertyChanged(nameof(HasSearchHistory));
        }

        private void LoadSearchHistory()
        {
            try
            {
                if (File.Exists(_searchHistoryPath))
                {
                    var lines = File.ReadAllLines(_searchHistoryPath);
                    searchHistory.Clear();
                    foreach (var line in lines.Take(20)) // Limit to 20 items
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            searchHistory.Add(line.Trim());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading search history: {ex.Message}");
            }

            OnPropertyChanged(nameof(HasSearchHistory));
        }

        private void SaveSearchHistory()
        {
            try
            {
                File.WriteAllLines(_searchHistoryPath, searchHistory);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving search history: {ex.Message}");
            }
        }

        private void CheckForSharedDocuments()
        {
            if (Directory.Exists(_gbyMailPath))
            {
                try
                {
                    var pdfFiles = Directory.GetFiles(_gbyMailPath, "*.pdf");
                    foreach (var pdfFile in pdfFiles)
                    {
                        var alreadyExists = ReceivedEmails.Any(email => email.PdfAttachment == pdfFile);
                        if (!alreadyExists)
                        {
                            var email = new EmailItem
                            {
                                Id = Guid.NewGuid(),
                                From = "GbyMail Documents",
                                Subject = Path.GetFileName(pdfFile),
                                Body = $"PDF found in GbyMail documents folder\nFile size: {GetFileSize(pdfFile)}\nFound by: JMTDI",
                                Date = File.GetCreationTime(pdfFile),
                                PdfAttachment = pdfFile
                            };
                            ReceivedEmails.Add(email);

                            // Auto-open the most recent PDF if none is selected
                            if (string.IsNullOrEmpty(SelectedPdfPath))
                            {
                                SelectedPdfPath = pdfFile;
                                SelectedTabIndex = 2;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error checking shared documents: {ex.Message}");
                }
            }

            RefreshVisibilityProperties();
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

        private void RefreshVisibilityProperties()
        {
            OnPropertyChanged(nameof(EmptyInboxVisibility));
            OnPropertyChanged(nameof(EmailListVisibility));
            OnPropertyChanged(nameof(NoPdfSelectedVisibility));
            OnPropertyChanged(nameof(PdfViewerVisibility));
            OnPropertyChanged(nameof(SelectedPdfName));
        }

        partial void OnSearchTextChanged(string value)
        {
            OnPropertyChanged(nameof(IsSearchEnabled));
        }

        partial void OnSelectedEmailChanged(EmailItem? value)
        {
            if (value?.PdfAttachment != null && File.Exists(value.PdfAttachment))
            {
                SelectedPdfPath = value.PdfAttachment;
                SelectedTabIndex = 2;
                RefreshVisibilityProperties();
            }
        }
    }
}