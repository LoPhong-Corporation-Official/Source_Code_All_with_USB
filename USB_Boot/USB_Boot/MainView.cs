using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Management;
using System.Diagnostics;
using System.Threading;


namespace USB_Boot
{
    public partial class MainView : DevExpress.XtraEditors.XtraForm
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeFileHandle CreateFileA(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        private SafeFileHandle _DriverHandle;
        private System.IO.FileStream _DriverStream;

        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint OPEN_EXISTING = 3;

        System.IO.DriveInfo[] DriverInfo;

        public MainView()
        {

            InitializeComponent();
            
            RefreshUSBList();
            comboBox2.Items.Add("GPT (GUID Partition Table)");
            comboBox2.Items.Add("MBR (Master Boot Record)");
            comboBox2.Enabled = false;
            comboBox2.SelectedIndexChanged += new EventHandler(comboBox2_SelectedIndexChanged);
            textBox3.Enabled = false;
            simpleButton1.Enabled = false;
            simpleButton2.Enabled = false;
           
        }

        void RefreshDrivers()
        {
            try
            {
                comboBox1.Items.Clear();

                DriveInfo[] drives = DriveInfo.GetDrives();

               
                foreach (DriveInfo drive in drives)
                {
               
                    if (drive.IsReady)
                    {
               
                        if (drive.DriveType == DriveType.Removable)
                        {
                   
                            comboBox1.Items.Add(string.Format("{0} [{1}]", drive.Name, drive.TotalSize / 1024 / 1024 / 1024 + " GB (Gigabyte)"));
                        }
                    }
                }

                _DriverHandle = CreateFileA("\\\\.\\H:", GENERIC_READ, FILE_SHARE_READ, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                _DriverStream = new System.IO.FileStream(_DriverHandle, System.IO.FileAccess.Read);

                byte[] bufferTest = new byte[512];
                _DriverStream.Read(bufferTest, 0, 512);

                _DriverStream.Close();
                _DriverHandle.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            simpleButton1.Enabled = true;
        }
        private void RefreshUSBList()
        {
            comboBox1.Items.Clear();

  
            DriveInfo[] drives = DriveInfo.GetDrives();

      
            foreach (DriveInfo drive in drives)
            {
          
                if (drive.IsReady)
                {
                 
                    if (drive.DriveType == DriveType.Removable)
                    {
                      
                        comboBox1.Items.Add(string.Format("{0} [{1}]" , drive.Name, drive.TotalSize / 1024 / 1024 / 1024 + " GB (Gigabyte)"));
                    }
                }
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
         
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "ISO Files (*.iso)|*.iso";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
       
                textBox1.Text = dialog.FileName;
             
                string fileName = Path.GetFileNameWithoutExtension(dialog.FileName);

       
                textBox3.Text = fileName;
                string isoPath = dialog.FileName;

                long isoSize = new FileInfo(isoPath).Length;

           
                textBox5.Text = isoSize.ToString() + " bytes";

              
            }
            
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox3.Enabled = true;
            

            if (comboBox2.SelectedIndex == 0)
            {
       
                textBox2.Text = "UEFI (Unified Extensible Firmware Interface)";
            }
            if (comboBox2.SelectedIndex == 1)
            {
                textBox2.Text = "BIOS (Basic Input Output System)";
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            comboBox2.Enabled = true;
            
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void MainView_Load(object sender, EventArgs e)
        {
           RefreshDrivers();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            simpleButton2.Enabled = true;
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            Close();
           
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you really want format USB? The data will be lost.", "LoPhong Corporation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.OK)
            {
                DialogResult result1 = MessageBox.Show("Do you sure your choice? This is last warning!", "LoPhong Corporation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result1 == DialogResult.OK)
                {
                
                    simpleButton4.Enabled = false;
                    comboBox1.Enabled = false;
                    simpleButton1.Enabled = false;
                    comboBox2.Enabled = false;
                    textBox3.Enabled = false;
                    simpleButton2.Enabled = false;
                    simpleButton3.Enabled = false;
                  
                    if (comboBox1.SelectedIndex == -1)
                    {
                        MessageBox.Show("Please choose a USB.", "LoPhong Corporation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    
                    string usbName = comboBox1.SelectedItem.ToString();
                    string driveLetter = usbName.Substring(0, 2);

                    if (comboBox2.SelectedIndex == 1)
                    {
                        try
                        {
                            Process process = new Process();
                            process.StartInfo.FileName = "cmd.exe";
                            process.StartInfo.Arguments = "/c format " + driveLetter + " /FS:NTFS /Q";
                            process.StartInfo.CreateNoWindow = true;
                            process.Start();
                            process.WaitForExit();


                            Thread.Sleep(5000);
                            string newUsbName = textBox3.Text;

                            Process process1 = new Process();
                            process1.StartInfo.FileName = "cmd.exe";
                            process1.StartInfo.Arguments = "/c label " + driveLetter + newUsbName;
                            process1.StartInfo.CreateNoWindow = true;
                            process1.Start();
                            process1.WaitForExit();


                            Thread.Sleep(5000);


                            // Get the selected ISO file path
                            string isoFilePath = textBox1.Text;

                            // Check if the ISO file exists
                            if (!File.Exists(isoFilePath))
                            {
                                MessageBox.Show("The ISO file is not exist. Please try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            // Check if a USB drive is selected
                            if (comboBox1.SelectedIndex == -1)
                            {
                                MessageBox.Show("Please select a removable before process.");
                                return;
                            }          
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to format USB. Please check and try again. ERROR MESSAGE:" + ex.Message, "LoPhong Corporation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            simpleButton4.Enabled = true;
                            comboBox1.Enabled = true;
                            simpleButton1.Enabled = true;
                            comboBox2.Enabled = true;
                            textBox3.Enabled = true;
                            simpleButton2.Enabled = true;
                            simpleButton3.Enabled = true;
                        }
                    }
                }
                else if (result1 == DialogResult.Cancel)
                {

                }
            }
            else if (result == DialogResult.Cancel)
            {

            }    
        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            RefreshDrivers();
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
           DialogResult result = MessageBox.Show("Please visit my Github. The About form is unsupported. Visit here: github.com/LoPhong-Corporation-Official", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if(result == DialogResult.Yes)
            {
                Process.Start("https://github.com/LoPhong-Corporation-Official");
            }

        }
       
    }
}
