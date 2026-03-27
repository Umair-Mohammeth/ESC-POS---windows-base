using System;
using System.Windows.Forms;

namespace POSWinForms
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                
                try {
                    Application.SetCompatibleTextRenderingDefault(false);
                } catch (InvalidOperationException) {
                    // SetCompatibleTextRenderingDefault can only be called before any controls are created.
                    // If a static initializer touched WinForms early, this will fail. We can safely ignore it.
                }

                Application.Run(new Views.LoginForm());
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null) msg += "\nInner: " + ex.InnerException.Message;
                
                MessageBox.Show($"Application crashed:\n\n{msg}\n\nStack Trace:\n{ex.StackTrace}",
                                "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
