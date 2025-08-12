using System.Linq;
using System.Windows;

namespace GbyMail
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Register file association (optional)
            FileAssociation.RegisterPdfAssociation();

            // Handle command line arguments for PDF files
            if (e.Args.Length > 0)
            {
                FileAssociation.HandleCommandLineArgs(e.Args);
            }
        }
    }
}