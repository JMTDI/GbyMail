using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace GbyMail
{
    public static class FileAssociation
    {
        public static void RegisterPdfAssociation()
        {
            try
            {
                var executablePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(executablePath)) return;

                // Register file association for PDF files
                using var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.pdf\shell\OpenWithGbyMail");
                key?.SetValue("", "Open with GbyMail");
                key?.SetValue("Icon", executablePath);
                
                using var commandKey = key?.CreateSubKey("command");
                commandKey?.SetValue("", $"\"{executablePath}\" \"%1\"");

                System.Diagnostics.Debug.WriteLine("PDF file association registered successfully - JMTDI - 2025-08-12 14:33:27");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not register file association: {ex.Message}");
            }
        }

        public static void HandleCommandLineArgs(string[] args)
        {
            if (args.Length > 0)
            {
                var pdfFiles = args.Where(arg => File.Exists(arg) && Path.GetExtension(arg).ToLower() == ".pdf").ToArray();
                
                if (pdfFiles.Length > 0)
                {
                    // Handle PDF files passed as command line arguments
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (Application.Current.MainWindow?.DataContext is MainViewModel viewModel)
                        {
                            // Import the first PDF file
                            viewModel.HandleSelectedPdf(pdfFiles[0]);
                            
                            // If multiple PDFs, import the rest as well
                            foreach (var pdfFile in pdfFiles.Skip(1))
                            {
                                viewModel.HandleSelectedPdf(pdfFile);
                            }
                        }
                    });
                }
            }
        }
    }
}