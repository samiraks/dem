using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace hostel
{
    public partial class Change_password : Form
    {
        private string login;
        private string connectionString = "Server=localhost;Port=3306;Database=hostel;Username=root;Password=root;";

        public Change_password(string userLogin)
        {
            InitializeComponent();
            this.login = userLogin;
           // this.FormClosing += Change_password_FormClosing;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            string old_pass = textBox1.Text;
            string new_pass = textBox2.Text;
            string new1_pass = textBox3.Text;


            if (string.IsNullOrEmpty(old_pass) || string.IsNullOrEmpty(new_pass) || string.IsNullOrEmpty(new1_pass))
            {
                MessageBox.Show("Все поля обязательны для заполнения", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (new_pass != new1_pass)
            {
                MessageBox.Show("Новый пароль и подтверждение не совпадают", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверка старого пароля
                    string checkQuery = "SELECT COUNT(*) FROM users WHERE login = @login AND password = @password";
                    using (var checkCmd = new MySqlCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@login", login);
                        checkCmd.Parameters.AddWithValue("@password", old_pass);

                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (count == 0)
                        {
                            MessageBox.Show("Неверный старый пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    // Обновление пароля
                    string updateQuery = @"UPDATE users SET password = @new_pass, is_first_login = FALSE, last_login = NOW() WHERE login = @login";
                    using (var updateCmd = new MySqlCommand(updateQuery, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@login", login);
                        updateCmd.Parameters.AddWithValue("@new_pass", new_pass);
                        // updateCmd.ExecuteNonQuery();

                        int affectedRows = updateCmd.ExecuteNonQuery();
                        if (affectedRows > 0)
                        {
                            MessageBox.Show("Пароль успешно изменен. Войдите с новым паролем.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                            
                        }
                        else
                        {
                            MessageBox.Show("Не удалось изменить пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка:\n{ex.Message}", "ОШибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Change_password_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK)
            {
                this.DialogResult = DialogResult.Cancel;
            }
        }
    }

}
