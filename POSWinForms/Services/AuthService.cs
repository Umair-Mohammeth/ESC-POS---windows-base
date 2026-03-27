using System.Linq;
using POSWinForms.Data;
using POSWinForms.Models;

namespace POSWinForms.Services
{
    public class AuthService
    {
        public User ValidatePin(string pin)
        {
            var ctx = DatabaseContext.Instance;
            // Simple fetch by PIN (for prototype use raw ADO.NET)
            var cmd = ctx.Connection.CreateCommand();
            cmd.CommandText = "SELECT id, name, pin, role FROM users WHERE pin = @pin LIMIT 1";
            cmd.Parameters.AddWithValue("@pin", pin);
            ctx.Connection.Open();
            using var reader = cmd.ExecuteReader();
            User user = null;
            if (reader.Read())
            {
                user = new User
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Pin = reader.GetString(2),
                    Role = reader.GetString(3)
                };
            }
            ctx.Connection.Close();
            return user;
        }
    }
}
