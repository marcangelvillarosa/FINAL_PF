using MySql.Data.MySqlClient;
using PfFinalProject;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Vonage.Request;
using Vonage;
using Pf_Final_Project;

namespace PF_Final
{
    public partial class Home : Form
    {
        public Home()
        {
            InitializeComponent();
        }

        public static FaceRecognitionSystem fr = Application.OpenForms.OfType<FaceRecognitionSystem>().FirstOrDefault();

        private void Home_Load(object sender, EventArgs e)
        {

            APIkey.Text = fr.label11.Text;
            APISecret.Text = fr.label12.Text;
            Contact.Text = fr.label14.Text;
            From.Text = fr.label13.Text;

            timer1.Start();
            label1.Text = DateTime.Now.ToLongTimeString();
            label2.Text = DateTime.Now.ToLongDateString();

            label3.Text = fr.label7.Text;

            string connectionString = "server=localhost;user=root;database=pf;password=;";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT Id, Face FROM users WHERE FullName = @FullName";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@FullName", label3.Text);

                MySqlDataAdapter sda = new MySqlDataAdapter(command);
                DataTable dta = new DataTable();
                sda.Fill(dta);

                string name = label3.Text;

                if (dta.Rows.Count == 0)
                {
                    Usernotfound usernotfound = new Usernotfound();
                    usernotfound.Show();
                }
                else
                {
                   string StudenId = dta.Rows[0]["Id"].ToString();
                   label4.Text = StudenId;

                    byte[] img = dta.Rows[0]["Face"] as byte[];
                    if (img != null && img.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(img))
                        {
                            try
                            {
                                pictureBox1.Image = Image.FromStream(ms);
                            }
                            catch (ArgumentException ex)
                            {
                                MessageBox.Show("Error: The image format is not valid. " + ex.Message);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("No image found for the selected student.");
                    }
                }
            }
        }

        private void DisplayImage(byte[] imageData)
        {
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                Image image = Image.FromStream(ms);
                pictureBox1.Image = image;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = DateTime.Now.ToLongTimeString();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            var credentials = Credentials.FromApiKeyAndSecret(
                       APIkey.Text,
                       APISecret.Text
                       ); 

            var VonageClient = new VonageClient(credentials);
            var response = VonageClient.SmsClient.SendAnSmsAsync(new Vonage.Messaging.SendSmsRequest()
            {
                To = Contact.Text,
                From = From.Text,
                Text = fr.label7.Text + " has currently left the premises of Holy Cross of Davao College. " 
            });
            MessageBox.Show("Guardian notified: You've left Holy Cross of Davao College. " + label1.Text + " " + label2.Text);

            FaceRecognitionSystem faceRecognitionSystem = new FaceRecognitionSystem();
            this.Hide();
            faceRecognitionSystem.Show();
        }

        private void Contact_Click(object sender, EventArgs e)
        {

        }
    }
}
