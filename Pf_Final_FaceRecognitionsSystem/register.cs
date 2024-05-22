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
using PfFinalProject;
using Pf_Final_Project;

namespace PfFinal
{
    public partial class register : Form
    {

        connection_class con = new connection_class();

     
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.6d, 0.6d);
        HaarCascade faceDetected;
        Capture camera;

        Image<Bgr, Byte> Frame;
        Image<Gray, byte> result;
        Image<Gray, byte> TrainedFace = null;
        Image<Gray, byte> grayFace = null;


        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> users = new List<string>();

        int Count, NumLables, t;
        string name, names = null;

        public register()
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

        private void register_Load(object sender, EventArgs e)
        {
            camera = new Capture();
            camera.QueryFrame();

            Application.Idle += new EventHandler(FrameProcedure);

            txName.Focus();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (txName.Text == "" || Email.Text == "" || Password.Text == "" || Contact.Text == "")
            {
                fillall fillall = new fillall();
                fillall.Show();
            }
            else
            {

                Count += 1;
                grayFace = camera.QueryGrayFrame().Resize(320, 240, INTER.CV_INTER_CUBIC);
                MCvAvgComp[][] DetectedFace = grayFace.DetectHaarCascade(faceDetected, 1.2, 10,
                    HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));

                foreach (MCvAvgComp f in DetectedFace[0])
                {

                    TrainedFace = Frame.Copy(f.rect).Convert<Gray, Byte>();
                    break;
                }

                TrainedFace = result.Resize(100, 100, INTER.CV_INTER_CUBIC);

                trainingImages.Add(TrainedFace);

                labels.Add(txName.Text);

                File.WriteAllText(Application.StartupPath + "/Faces/Faces.txt", trainingImages.ToArray().Length.ToString() + ",");

                for (int i = 1; i < trainingImages.ToArray().Length + 1; i++)
                {
                    trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/Faces/face" + i + ".bmp");
                    File.AppendAllText(Application.StartupPath + "/Faces/Faces.txt", labels.ToArray()[i - 1] + ",");
                }

                facestored facestored = new facestored();
                facestored.Show();
                txName.Focus();


                byte[] imageData = ConvertImageToBytes(TrainedFace);
                string connectionString = "server=localhost;user=root;database=pf;password=;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();


                    string query = "INSERT INTO users (Face, FullName, Email, Password, ContactNo) VALUES (@imageData, @FullName, @Email, @Password, @ContactNo)";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.Add("@imageData", MySqlDbType.LongBlob).Value = imageData;
                    command.Parameters.Add("@FullName", MySqlDbType.VarChar).Value = txName.Text;
                    command.Parameters.Add("@Email", MySqlDbType.VarChar).Value = Email.Text;
                    command.Parameters.Add("@Password", MySqlDbType.VarChar).Value = Password.Text;
                    command.Parameters.Add("@ContactNo", MySqlDbType.Int64).Value = Contact.Text;



                    int rowsAffected = command.ExecuteNonQuery();
                    txName.Text = "";
                    Email.Text = "";
                    Password.Text = "";
                    Contact.Text = "";
                    
                    connection.Close();
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void register_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
           
        }

        private void IBOutput_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            FaceRecognitionSystem faceRecognitionSystem = new FaceRecognitionSystem();
            this.Hide();
            faceRecognitionSystem.Show();
        }

      



        private byte[] ConvertImageToBytes(Image<Gray, byte> image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
           
                image.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            
                return ms.ToArray();
            }
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



               
                Frame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.White));

              
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
