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
                Application.SetCompatibleTextRenderingDefault(false);
                // Start with login screen so user can authenticate by PIN
                Application.Run(new Views.LoginForm());
            }
            catch (Exception)
            {
                // Silent catch or simplified report
            }
        }
    }
}
