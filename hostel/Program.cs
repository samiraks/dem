using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace hostel
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Authorization());
        }
        public class AuthService
        {
            private string connectionString = "server=localhost;user=root;database=library;port=3306;password=root"; // Укажите правильные данные для подключения

            // Метод для аутентификации и получения роли пользователя
            public string AuthenticateAndGetRole(string login, string password)
            {
                string role = null; // Лучше инициализировать как null
                                    //  string role = string.Empty;
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        // Запрос для получения роли пользователя по логину и паролю
                        string query = "SELECT role FROM hostel.users WHERE login = @login AND password = @password";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@login", login);
                        command.Parameters.AddWithValue("@password", password);

                        // Используем ExecuteScalar вместо ExecuteReader для простых запросов
                        object result = command.ExecuteScalar();

                        // Безопасное преобразование
                        role = result != null && result != DBNull.Value ? result.ToString() : null;

                        MySqlDataReader reader = command.ExecuteReader();

                        if (reader.Read()) // Если пользователь найден
                        {
                            role = reader["role"].ToString(); // Получаем роль из базы данных
                        }
                        else
                        {
                            role = null; // Если данных нет (неверный логин или пароль)
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка подключения к базе данных: " + ex.Message);
                    }
                }

                return role;
            }
        }
    }
}
