using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static hostel.Program;

namespace hostel
{
    public partial class Authorization : Form
    {
        string connectionString = "Server = localhost; Port = 3306; Database = hostel; Username = root; Password = root;";
        public Authorization()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text;
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Поля 'Логин' и 'Пароль' обязательны для заполнения. Пожалуйста, повторите попытку!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    
                    //Проверка существования пользователя
                    string query = "SELECT COUNT(*) FROM hostel.users WHERE login = @login";
                    using (MySqlCommand userExistsCmd = new MySqlCommand(query, connection))
                    {
                        userExistsCmd.Parameters.AddWithValue("@login", login);
                        int userCount = Convert.ToInt32(userExistsCmd.ExecuteScalar());

                        if (userCount == 0)
                        {
                            MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                  
                    //Проверка учетной записи и аутентификации
                    string authQuery = "SELECT role, status, login_attempts, last_login, is_first_login FROM hostel.users WHERE login = @login AND password=@password";
                    using (MySqlCommand authCmd = new MySqlCommand(authQuery, connection))
                    {
                        authCmd.Parameters.AddWithValue("@login", login);
                        authCmd.Parameters.AddWithValue("@password", password);
                        using (MySqlDataReader reader = authCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Успешная аутентификация
                                string status = reader["status"].ToString();

                                // Проверка блокировки
                                if (status == "Заблокирован")
                                {
                                    MessageBox.Show("Аккаунт заблокирован. Обратитесь к администратору.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                                // Проверка неактивности
                                DateTime lastLogin = reader["last_login"] != DBNull.Value ? Convert.ToDateTime(reader["last_login"]) : DateTime.MinValue;

                                if ((DateTime.Now - lastLogin).TotalDays > 30 && lastLogin != DateTime.MinValue)
                                {
                                    BlockUser(login);
                                    MessageBox.Show("Аккаунт заблокирован из-за неактивности.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                string role = reader["role"] != DBNull.Value ? reader["role"].ToString() : null;
                                bool is_first_login = reader["is_first_login"] == DBNull.Value || Convert.ToBoolean(reader["is_first_login"]);

                                ResetLoginAttempts(login);
                                UpdateLastLogin(login);

                                if (is_first_login)
                                {
                                    txtLogin.Clear();
                                    txtPassword.Clear();  
                                    using (Change_password changePass = new Change_password(login))
                                    {
                                        if (changePass.ShowDialog() == DialogResult.OK)
                                        {
                                           //UpdateFirstLoginStatus(login, false);
                                           //this.Close(); // Закрываем форму авторизации после успешной смены пароля
                                        }
                                        else
                                        {
                                            this.Show(); // Показываем форму авторизации снова при отмене
                                        }
                                        
                                    }
                                }
                                
                                else
                                {
                                    MessageBox.Show("Вы успешно авторизовались!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    //OpenDashboard(role);
                                    // Здесь переключаем на основную форму
                                    AuthService authService = new AuthService();
                                    string userRole = authService.AuthenticateAndGetRole(login, password);
                                    mainForm MainForm = new mainForm(userRole);
                                    this.Hide();
                                    MainForm.ShowDialog();
                                    this.Close();
                                }
                            }
                            else
                            {
                                IncrementLoginAttempts(login);
                                //Проверяем количество попыток и блокируем при необходимости
                                int attempts = GetLoginAttempts(login);
                                if (attempts >= 3)
                                {
                                    BlockUser(login);
                                    MessageBox.Show("Аккаунт заблокирован из-за слишком большого количества неудачных попыток входа. Обратитесь к администратору", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                else
                                {
                                    MessageBox.Show("Неверный логин или пароль. Повторите попытку", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ResetLoginAttempts(string login)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand("UPDATE users SET login_attempts = 0 WHERE login = @login", connection))
            {
                connection.Open();
                cmd.Parameters.AddWithValue("@login", login);
                cmd.ExecuteNonQuery();
            }
        }
        private void IncrementLoginAttempts(string login)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand("UPDATE users SET login_attempts = login_attempts + 1 WHERE login = @login", connection))
            {
                connection.Open();
                cmd.Parameters.AddWithValue("@login", login);
                cmd.ExecuteNonQuery();
            }
        }
        private void BlockUser(string login)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand("UPDATE users SET status = 'Заблокирован' WHERE login = @login", connection))
            {
                connection.Open();
                cmd.Parameters.AddWithValue("@login", login);
                cmd.ExecuteNonQuery();
            }
        }
        private void UpdateLastLogin(string login)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand("UPDATE users SET last_login = NOW() WHERE login = @login", connection))
            {
                connection.Open();
                cmd.Parameters.AddWithValue("@login", login);
                cmd.ExecuteNonQuery();
            }
        }
        private int GetLoginAttempts(string login)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand("SELECT login_attempts FROM users WHERE login = @login", connection))
            {
                connection.Open();
                cmd.Parameters.AddWithValue("@login", login);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        private void UpdateFirstLoginStatus(string login, bool is_first_login)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand("UPDATE users SET is_first_login = @is_first_login WHERE login = @login", connection))
            {
                connection.Open();
                cmd.Parameters.AddWithValue("@is_first_login", is_first_login);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
