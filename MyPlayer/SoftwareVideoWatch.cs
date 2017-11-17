using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MyPlayer
{
    public partial class frmMyPlayer : Form
    {
        public frmMyPlayer()
        {
            InitializeComponent();
        }

        public string ConnectionString
        {
            get { return "Data Source=112.196.9.213;Initial Catalog=Videos;Persist Security Info=True;User ID=sa;Password=Admin123;"; }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var file = GetFile();
            var lst = new string[] { ".mp3", ".avi", ".mp4", ".wmv" };
            if (!lst.Contains(Path.GetExtension(file)))
            { MessageBox.Show("Please select proper file."); }
            else
            {
                var result = SaveToDataBase(Path.GetFileName(file), GetCompressedData(file, ConvertFileToByteData(file)));
                if (result)
                {
                    cmbPlayList.Items.Add(Path.GetFileName(file));
                    MessageBox.Show("!! File Saved Successfully !!");
                }
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)

        {
            DialogResult dialogResult = MessageBox.Show("Are you sure to download the Software?", "Alert!", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                if (cmbPlayList.SelectedItem == null) { MessageBox.Show("Please select file to play."); return; }
                axWindowsMediaPlayer1.URL = GetFromDataBase(cmbPlayList.SelectedItem.ToString());
                axWindowsMediaPlayer1.settings.autoStart = true;
            }
            else if (dialogResult == DialogResult.No)
            {
                return;
            }
        }

        private void frmMyPlayer_Load(object sender, EventArgs e)
        {
            try
            {
                SqlConnection myConnection = new SqlConnection(ConnectionString);

                String Query1 = "SELECT FileName FROM [Videos].[dbo].[MyPlay]";

                SqlDataAdapter adapter = new SqlDataAdapter(Query1, ConnectionString);

                DataSet Ds = new DataSet();

                adapter.Fill(Ds, "MyPlay");

                if (Ds.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("No data Found");
                }
                else
                {
                    foreach (DataRow item in Ds.Tables[0].Rows)
                    {
                        cmbPlayList.Items.Add(item["FileName"]);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cmbPlayList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void frmMyPlayer_Resize(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Height = ((Form)sender).Size.Height - 145;
            axWindowsMediaPlayer1.Width = ((Form)sender).Size.Width - 40;
        }

        private void frmMyPlayer_MaximumSizeChanged(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Height = ((Form)sender).Size.Height - 145;
            axWindowsMediaPlayer1.Width = ((Form)sender).Size.Width - 40;
        }

        private void frmMyPlayer_MinimumSizeChanged(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Height = ((Form)sender).Size.Height - 145;
            axWindowsMediaPlayer1.Width = ((Form)sender).Size.Width - 40;
        }

        private void frmMyPlayer_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private string GetFile()
        {
            try
            {
                ofdPlayer.Filter = "Solution Files (*.*)|*.*";
                //ofdPlayer.InitialDirectory = @"E:\\";
                ofdPlayer.Multiselect = true;
                if (ofdPlayer.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return ofdPlayer.FileName;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return string.Empty;
        }

        private bool SaveToDataBase(string fileName, byte[] data)
        {
            try
            {
                var ds = new DataSet();
                SqlCommand cmd = new SqlCommand("insert into MyPlay values('" + Guid.NewGuid() + "','" + fileName + "',@content)");
                SqlParameter param = cmd.Parameters.Add("@content", SqlDbType.VarBinary);
                param.Value = data;
                cmd.Connection = new SqlConnection(ConnectionString);
                cmd.CommandTimeout = 0;
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }

        private string GetFromDataBase(string fileName)
        {
            try
            {
                SqlConnection myConnection = new SqlConnection(ConnectionString);
                String Query1 = "SELECT FileData FROM [Videos].[dbo].[MyPlay] where FileName = '" + fileName + "'";
                SqlDataAdapter adapter = new SqlDataAdapter(Query1, ConnectionString);
                DataSet Ds = new DataSet();
                adapter.Fill(Ds, "MyPlay");
                if (Ds.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("No data Found");
                    return string.Empty;
                }
                return ConvertByteDataToFile(fileName, GetUnCompressedData((byte[])Ds.Tables[0].Rows[0]["FileData"]));
            }
            catch (Exception)
            {
                throw;
            }
            return string.Empty;
        }

        private string ConvertByteDataToFile(string targetFileName, byte[] value)
        {
            // ReSharper disable EmptyGeneralCatchClause
            var str = string.Empty;
            try
            {
                try
                {
                    var path = Path.GetTempPath();
                    str = path + "\\" + targetFileName;
                    if (File.Exists(str))
                        File.Delete(str);
                }
                catch (Exception) { }

                var file = (new BinaryWriter(new FileStream(str, FileMode.OpenOrCreate, FileAccess.Write)));
                file.Write(value);
                file.Close();
                return str;
            }
            catch (Exception) { }
            // ReSharper restore EmptyGeneralCatchClause
            return string.Empty;
        }

        private static byte[] ConvertFileToByteData(string sourceFileName)
        {
            BinaryReader binaryReader = null;
            if (!File.Exists(sourceFileName))
                return null;

            try
            {
                binaryReader = new BinaryReader(new FileStream(sourceFileName, FileMode.Open, FileAccess.Read));
                return binaryReader.ReadBytes(ConvertToInt32(binaryReader.BaseStream.Length));
            }
            finally
            {
                if (null != binaryReader) binaryReader.Close();
            }
        }

        public static int ConvertToInt32(object parameter)
        {
            var returnvalue = Int32.MinValue;

            // If exception (Input string was not in a correct format) 
            // is raised then default return value is returned.
            try
            {
                if (null != parameter)
                    returnvalue = Convert.ToInt32(parameter, CultureInfo.InvariantCulture);
            }
            catch
            {
                return returnvalue;
            }

            return returnvalue;
        }

        public static byte[] GetCompressedData(string fileName, byte[] value)
        {
            try
            {
                // Code for zip file.
                if (value != null && !string.IsNullOrEmpty(fileName))
                    using (var zippedMemoryStream = new MemoryStream())
                    {
                        // A ZIP stream
                        using (var zipOutputStream = new ZipOutputStream(zippedMemoryStream))
                        {
                            // Highest compression rating 0 - 9.
                            zipOutputStream.SetLevel(9);

                            var entry = new ZipEntry(fileName) { DateTime = DateTime.Now };
                            zipOutputStream.PutNextEntry(entry);

                            zipOutputStream.Write(value, 0, ConvertToInt32(value.Length));

                            zipOutputStream.Finish();
                            zipOutputStream.Close();

                            return zippedMemoryStream.ToArray();
                        }
                    }
            }
            catch (Exception)
            {
                return value;
            }

            return null;
        }

        public static byte[] GetUnCompressedData(byte[] value)
        {
            try
            {
                if (value != null)
                    using (var zipInputStream = new ZipInputStream(new MemoryStream(value)))
                    {
                        while ((zipInputStream.GetNextEntry()) != null)
                        {
                            using (var zippedInMemoryStream = new MemoryStream())
                            {
                                var data = new byte[2048];
                                while (true)
                                {
                                    var size = zipInputStream.Read(data, 0, data.Length);
                                    if (size <= 0)
                                        break;

                                    zippedInMemoryStream.Write(data, 0, size);
                                }
                                zippedInMemoryStream.Close();

                                return zippedInMemoryStream.ToArray();
                            }
                        }
                    }
                return null;
            }
            catch (Exception)
            {
                return value;
            }
        }

        private void axWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
          //  MessageBox.Show("here is Exe file you can Upload!");



            //DialogResult dialogResult = MessageBox.Show("Alert!", "Please Click yes to download the Software", MessageBoxButtons.YesNo);
            //if (dialogResult == DialogResult.Yes)
            //{
            //    MessageBox.Show("Software Downloaded");


            //}
            //else if (dialogResult == DialogResult.No)
            //{
            //    MessageBox.Show("Please Download the Software First");
            //    return;
            //}

        }

        private void axWindowsMediaPlayer1_ClickEvent(object sender, AxWMPLib._WMPOCXEvents_ClickEvent e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure to download the Software?", "Alert!", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                if (cmbPlayList.SelectedItem == null) { MessageBox.Show("Please select file to play."); return; }
                axWindowsMediaPlayer1.URL = GetFromDataBase(cmbPlayList.SelectedItem.ToString());
                axWindowsMediaPlayer1.settings.autoStart = true;
            }
            else if (dialogResult == DialogResult.No)
            {
                return;
            }
          
        }
    }
}

