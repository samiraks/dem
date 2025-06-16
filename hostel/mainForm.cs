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
    public partial class mainForm : Form
    {
        private string connectionString = "Server=localhost;Port=3306;Database=hostel;Username=root;Password=root;";
        private string role;
        public mainForm(string userRole)
        {
            InitializeComponent();
            //InitializePlaceholder();
            this.role = userRole;
            ConfigureFormBasedOnRole();
            lblRole.Text = $"Ваша роль: {this.role}";
        }

        private void ConfigureFormBasedOnRole()
        {
            if (this.role == "Администратор")
            {
                button1.Visible = true;
            }
            else
            {
                button1.Visible = false;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            controlForm Controlform = new controlForm();
            //this.Hide();
            Controlform.ShowDialog();
            //this.Close();
        }
    }
}
