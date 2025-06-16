using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace hostel
{
    public partial class controlForm : Form
    {
        private string connectionString = "Server = localhost; Port = 3306; Username = root; Password = root";

        public controlForm()
        {
            InitializeComponent();
            ShowClientInGrid();
        }
        void ShowClientInGrid()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "SELECT * FROM hostel.users";
            MySqlDataAdapter adapter = new MySqlDataAdapter(query,connection);
            DataTable table = new DataTable();
            adapter.Fill(table);
            dataGridView1.DataSource = table;
            connection.Close();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string login = textBox1.Text;
            string password = textBox2.Text;
            string role = comboBox1.Text;
            string status = comboBox2.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role) || string.IsNullOrEmpty(status)) 
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            //добавление нового пользователя
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO hostel.users (login, password, role, status)" + "VALUES (@login,@password,@role, @status)";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@role", role);
                        cmd.Parameters.AddWithValue("@status", status);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Пользователь успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex) 
                {
                    MessageBox.Show ($"Ошибка подключения: {ex.Message}","Ошибка",MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            string login = textBox1.Text;
            string password = textBox2.Text;
            string role = comboBox1.Text;
            string status = comboBox2.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role) || string.IsNullOrEmpty(status))
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            //Изменение пользователя
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                if (!string.IsNullOrEmpty(password))
                {
                    string updatePasswordQuery = "UPDATE hostel.users SET password = @password, role=@role, status=@status WHERE login = @login";
                    using (var cmd = new MySqlCommand(updatePasswordQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@role", role);
                        cmd.Parameters.AddWithValue("@status",status);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Данные пользователя успешно изменены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Пользователь не найден!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            string login = textBox1.Text;
            //string password = textBox2.Text;
            //string role = comboBox1.Text;
            string status = comboBox2.Text;

            if (string.IsNullOrEmpty(login) ||  string.IsNullOrEmpty(status))
            {
                MessageBox.Show("Заполните все обязательные поля: логин и статус!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            //Разблокировка пользователя
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE hostel.users SET login_attempts = 0 WHERE login=@login";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@login", login);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Пользователь успешно разблокирован", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Пользователь не найден!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при разблокировки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            string login = textBox1.Text;
            //string password = textBox2.Text;
            string role = comboBox1.Text;
            string status = comboBox2.Text;
            if (string.IsNullOrEmpty(login) && string.IsNullOrEmpty(role) && string.IsNullOrEmpty(status))
            {
                MessageBox.Show("Заполните хотя бы одно поле для поиска", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string query = "SELECT * FROM hostel.users WHERE 1=1";
            if (!string.IsNullOrEmpty(login)) query += " AND login LIKE @login";
            if (!string.IsNullOrEmpty(role)) query += " AND role LIKE @role";
            if (!string.IsNullOrEmpty(status)) query += " AND status LIKE @status";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    if (!string.IsNullOrEmpty(login))
                    {
                        cmd.Parameters.AddWithValue("@login", "%" + login + "%");
                    }
                    if (!string.IsNullOrEmpty(role))
                    {
                        cmd.Parameters.AddWithValue("@role", "%" + role + "%");
                    }
                    if(!string.IsNullOrEmpty(status))
                    {
                        cmd.Parameters.AddWithValue("@status", "%" + status + "%");
                    }
                        
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    dataGridView1.DataSource = table;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при поиске: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            string login = textBox1.Text;   
            string password = textBox2.Text;
            string role = comboBox1.Text;
            string status = comboBox2.Text;

            //Проверяем выбрана ли книга на датагрид
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите пользователя для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            //получаем id выбранного пользователя
            int userId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["id"].Value);
            string userLogin = dataGridView1.SelectedRows[0].Cells["login"].Value.ToString();

            //Запрашиваем подтверждение
            DialogResult result = MessageBox.Show($"Вы уверены, что хотите удалить данные о пользователе:\n\n\"{userLogin}\"?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = "DELETE FROM hostel.users WHERE id = @id";
                        using (var cmd = new MySqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@id", userId);
                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Данные о пользователе успешно удалены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Не удалось удалить данные пользователя!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
