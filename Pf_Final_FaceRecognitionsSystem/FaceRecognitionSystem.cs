using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using PfFinalProject.myclass;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using PfFinal;
using System.Xml.Linq;
using Vonage;
using Vonage.Request;
using PF_Final;
using Pf_Final_Project;

namespace PfFinalProject
{
    public partial class FaceRecognitionSystem : Form
    {


        connection_class con = new connection_class();


        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.6d, 0.6d);

        HaarCascade faceDetected;
        Capture camera;
        Image<Bgr, Byte> Frame;
        Image<Gray, byte> result;
        Image<Gray, byte> grayFace = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();

        List<string> labels = new List<string>();
        List<string> users = new List<string>();

        int Count, NumLables, t;
        string name, names = null;

        public FaceRecognitionSystem()
        {

            InitializeComponent();


            faceDetected = new HaarCascade("haarcascade_frontalface_alt.xml");
            
            try
            {
                string Labelsinf = File.ReadAllText(Application.StartupPath + "/Faces/Faces.txt");
                string[] Labels = Labelsinf.Split(',');

                NumLables = Convert.ToInt16(Labels[0]);
                Count = NumLables;

                string FacesLoad;

                for (int i = 1; i < NumLables + 1; i++)
                {
                    FacesLoad = "face" + i + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/Faces/" + FacesLoad));
                    labels.Add(Labels[i]);
                }

            }
            catch (Exception)
            {
               
            }

        }
       
        private void Open_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = Application.StartupPath + "/Faces/",
                UseShellExecute = true,
                Verb = "open"
            });

        }

        private void FaceRecognitionSystem_Load(object sender, EventArgs e)
        {
            camera = new Capture();
            camera.QueryFrame();

            Application.Idle += new EventHandler(FrameProcedure);

            panel2.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string connectionString = "server=localhost;user=root;database=pf;password=;";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT ContactNo FROM users WHERE FullName = @FullName";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@FullName", lblName.Text.Trim());

                MySqlDataAdapter sda = new MySqlDataAdapter(command);
                DataTable dta = new DataTable();
                sda.Fill(dta);

                string name = lblName.Text.Trim();

                if (dta.Rows.Count == 0)
                {
                    
                    Usernotfound usernotfound = new Usernotfound();
                    usernotfound.Show();
                }
                else
                {
                    string contactNo = dta.Rows[0]["ContactNo"].ToString();
            
                    if (!contactNo.StartsWith("+"))
                    {
                       
                        contactNo = "+" + contactNo;
                    }
                    label14.Text = contactNo;
                    label7.Text = name;
                    panel2.Visible = true;
                }
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            string email = Email.Text.Trim();
            string password = Password.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                enterpassandemail enterpassandemail = new enterpassandemail();
                enterpassandemail.Show();
                return;
            }

            string connectionString = "server=localhost;user=root;database=pf;password=;";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM users WHERE FullName = @FullName";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@FullName", label7.Text.Trim());

                MySqlDataAdapter sda = new MySqlDataAdapter(command);
                DataTable dta = new DataTable();
                sda.Fill(dta);

                if (dta.Rows.Count == 0)
                {
                    usernotrecognized usernotrecognized = new usernotrecognized();
                    usernotrecognized.Show();
                }
                else
                {
                    string dbEmail = dta.Rows[0]["Email"].ToString();
                    string dbPassword = dta.Rows[0]["Password"].ToString();

                    if (dbEmail == email && dbPassword == password)
                    {
                        Home home = new Home();
                        this.Hide();
                        home.Show();


                        var credentials = Credentials.FromApiKeyAndSecret(
                        label11.Text,
                        label12.Text
                        );


                        var VonageClient = new VonageClient(credentials);
                        var response = VonageClient.SmsClient.SendAnSmsAsync(new Vonage.Messaging.SendSmsRequest()
                        {
                            To = label14.Text,
                            From = label13.Text,
                            Text = label7.Text + " has currently arrived at the premises of Holy Cross of Davao College."
                        });
                        guardiannotified guardiannotified = new guardiannotified();
                        guardiannotified.Show();
                    }
                    else
                    {
                        invalidcredentials invalidcredentials = new invalidcredentials();
                        invalidcredentials.Show();
                    }
                }
            }
        }

        private void cameraBox_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {
            panel2.Visible = false;
        }

        private void panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {

        }



        private void label6_Click(object sender, EventArgs e)
        {
            register register = new register();
            this.Hide();
            register.Show();
        }


        private void FrameProcedure(object sender, EventArgs e)
        {
            lblCountAllFaces.Text = "0";

            Frame = camera.QueryFrame().Resize(320, 240, INTER.CV_INTER_CUBIC);
            grayFace = Frame.Convert<Gray, Byte>();

            MCvAvgComp[][] faceDetectedShow = grayFace.DetectHaarCascade(faceDetected, 1.2, 10,
                HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));

            
            users.Clear();

            foreach (MCvAvgComp f in faceDetectedShow[0])
            {
                t += 1;

                result = Frame.Copy(f.rect).Convert<Gray, Byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);
                Frame.Draw(f.rect, new Bgr(Color.Blue), 1);

                if (trainingImages.ToArray().Length != 0)
                {


                    MCvTermCriteria termCriterias = new MCvTermCriteria(Count, 0.001);
                    EigenObjectRecognizer recognizer =
                        new EigenObjectRecognizer(trainingImages.ToArray(),
                        labels.ToArray(), 3000,
                        ref termCriterias);

                    name = recognizer.Recognize(result);

                   
                    if (string.IsNullOrEmpty(name))
                        name = "Not Registered";

                }
               
                else
                {
                    name = "Not Registered";
                }



              
                Frame.Draw(name, ref font, new Point(f.rect.X - 5, f.rect.Y - 10), new Bgr(Color.White));

                
                users.Add(name);
                users.Add(""); 
            }

            
            t = 0;

            
            lblCountAllFaces.Text = faceDetectedShow[0].Length.ToString();

            
            foreach (string user in users)
            {
                names += user;
            }

            cameraBox.Image = Frame;
            lblName.Text = names;

            
            names = "";
        }




    }


}
