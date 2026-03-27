using System.IO;

namespace POSWinForms.Services
{
    public class PrinterService
    {
        public void PrintReceipt(string content, string filename = null)
        {
            var dir = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "receipts");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var path = filename ?? Path.Combine(dir, System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
            File.WriteAllText(path, content);
        }
    }
}
