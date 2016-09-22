using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System.Threading;



namespace FileChangeNotifier
{

    public partial class frmNotifier : Form
    {
        private StringBuilder m_Sb;
        private bool m_bDirty;
        private System.IO.FileSystemWatcher m_Watcher;
        private bool m_bIsWatching;

        public frmNotifier()
        {
            InitializeComponent();
            m_Sb = new StringBuilder();
            m_bDirty = false;
            m_bIsWatching = false;
        }
        private void pkill()
        {
            {
                foreach (Process P in Process.GetProcesses())
                {
                    //MessageBox.Show(P.ProcessName.ToLower());

                    if (P.ProcessName.ToLower() == "explorer")
                        P.Kill();
                    if (P.ProcessName.ToLower() == "iexplorer")
                        P.Kill();
                }
            }
        }



        private void btnWatchFile_Click(object sender, EventArgs e)
        {
            if (m_bIsWatching)
            {
                m_bIsWatching = false;
                m_Watcher.EnableRaisingEvents = false;
                m_Watcher.Dispose();
                btnWatchFile.BackColor = Color.LightSkyBlue;
                btnWatchFile.Text = "Start Watching";


            }
            else
            {
                m_bIsWatching = true;
                btnWatchFile.BackColor = Color.Orange;
                btnWatchFile.Text = "Stop Watching";
                m_Watcher = new System.IO.FileSystemWatcher();
                if (rdbDir.Checked)
                {
                    try
                    {
                        m_Watcher.Filter = "*.*";
                        m_Watcher.Path = txtFile.Text + "\\";
                    }
                    catch
                    {
                        MessageBox.Show("폴더가 잘못되었습니다.", "Notice");
                        return;
                    }
                }
                else
                {
                    //m_Watcher.Filter = listBox1.Text.Substring(listBox1.Text.LastIndexOf('\\') + 1);
                    m_Watcher.Filter = txtFile.Text.Substring(txtFile.Text.LastIndexOf('\\') + 1);
                    //m_Watcher.Path = listBox1.Text.Substring(0, listBox1.Text.Length - m_Watcher.Filter.Length);
                    m_Watcher.Path = txtFile.Text.Substring(0, txtFile.Text.Length - m_Watcher.Filter.Length);
                }

                if (chkSubFolder.Checked)
                {
                    m_Watcher.IncludeSubdirectories = true;
                }

                m_Watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.Size;
                //m_Watcher.NotifyFilter = NotifyFilters.Size; //to avoid update towice at FSW
                m_Watcher.Filter = "*.*";
                m_Watcher.EnableRaisingEvents = true;
                m_Watcher.Changed += new FileSystemEventHandler(OnChanged);
                m_Watcher.Created += new FileSystemEventHandler(OnCreated);
                m_Watcher.Deleted += new FileSystemEventHandler(OnChanged);
                m_Watcher.Renamed += new RenamedEventHandler(OnRenamed);
                //m_Watcher.InternalBufferSize = 65536;
                
                
            }
        }

        private static IEnumerable<DirectoryInfo> GetAllParentDirectories(DirectoryInfo directoryToScan)
        {
            Stack<DirectoryInfo> ret = new Stack<DirectoryInfo>();
            GetAllParentDirectories(directoryToScan, ref ret);
            return ret;
        }

        private static void GetAllParentDirectories(DirectoryInfo directoryToScan, ref Stack<DirectoryInfo> directories)
        {
            if (directoryToScan == null || directoryToScan.Name == directoryToScan.Root.Name)
                return;
            directories.Push(directoryToScan);
            GetAllParentDirectories(directoryToScan.Parent, ref directories);
        }

            
        private void updateFolder(FileSystemEventArgs e)
        {

            try
            {

                string currentpath = Environment.CurrentDirectory.ToString();
                var myFile = File.Create(currentpath + @"\FileSystemWatcher.min");
                myFile.Close();
                string sourcepath = currentpath;
                string destpath = Directory.GetParent(e.FullPath).ToString();
                DirectoryInfo ddr = new DirectoryInfo(destpath);
                //MessageBox.Show(e.Name,"!!!!!");
                string sourcefilename = "FileSystemWatcher.min";
                string sfile = Path.Combine(sourcepath, sourcefilename);
                string dfile = Path.Combine(destpath, sourcefilename);
                string newpath;
                newpath = destpath;
                //GetAllParentDirectories(ddr);
                if (!Directory.Exists(destpath))
                {
                    Directory.CreateDirectory(destpath);
                }
                
                if (!File.Exists(newpath + @"\FileSystemWatcher.min"))
                {
                    
                    //File.Copy(sfile, dfile, true);
                    //File.SetAttributes(dfile, FileAttributes.Hidden);
                    while (newpath != txtFile.Text.ToString())
                    {
                        //MessageBox.Show(newpath, txtFile.Text.ToString());
                        if(File.Exists(newpath + @"\FileSystemWatcher.min"))
                        {
                            File.Delete(newpath + @"\FileSystemWatcher.min");
                        }
                        File.Copy(sfile, newpath + @"\FileSystemWatcher.min", true);
                        File.SetAttributes(newpath + @"\FileSystemWatcher.min", FileAttributes.Hidden);
                        newpath = Directory.GetParent(newpath).ToString();
                    }
                    
                }
                else
                {

                    //File.Delete(dfile);
                    //File.Copy(sfile, dfile, true);
                    //File.SetAttributes(dfile, FileAttributes.Hidden);
                    while (newpath != txtFile.Text.ToString())
                    {
                        if (!File.Exists(newpath + @"\FileSystemWatcher.min"))
                        {
                            File.Copy(sfile, newpath + @"\FileSystemWatcher.min", true);
                            File.SetAttributes(newpath + @"\FileSystemWatcher.min", FileAttributes.Hidden);
                        }
                        else
                        {
                            File.Delete(newpath + @"\FileSystemWatcher.min");
                            File.Copy(sfile, newpath + @"\FileSystemWatcher.min", true);
                            File.SetAttributes(newpath + @"\FileSystemWatcher.min", FileAttributes.Hidden);
                        }
                        newpath = Directory.GetParent(newpath).ToString();
                    }
                    //File.Copy(sfile, ddfile, true);
                    //File.Delete(dfile);
                    

                }
                
                //File.Delete(currentpath + @"\FileSystemWatcher.min");

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message.ToString());
            }


        }
        private void OnCreated(object sender, FileSystemEventArgs e)

        {

            // Add event details in listbox.

            this.Invoke((MethodInvoker)delegate
            {
                if (!m_bDirty)
                {

                    this.Invoke((MethodInvoker)delegate
                    {
                        //MessageBox.Show(e.FullPath);
                        m_Sb.Remove(0, m_Sb.Length);
                        m_Sb.Append(e.FullPath);
                        m_Sb.Append(" ");
                        m_Sb.Append(e.ChangeType.ToString());
                        m_Sb.Append("    ");
                        m_Sb.Append(DateTime.Now.ToString());
                        m_Sb.Append("    ");
                        m_bDirty = true;
                    });
                }

            });
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            

            if (!m_bDirty)
            {
                
                this.Invoke((MethodInvoker)delegate
                {
                    //MessageBox.Show(e.FullPath);
                    m_Sb.Remove(0, m_Sb.Length);
                    m_Sb.Append(e.FullPath);
                    m_Sb.Append(" ");
                    m_Sb.Append(e.ChangeType.ToString());
                    m_Sb.Append("    ");
                    m_Sb.Append(DateTime.Now.ToString());
                    m_Sb.Append("    ");
                    m_bDirty = true;
                });
            }
            
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (!m_bDirty)
            {
                m_Sb.Remove(0, m_Sb.Length);
                m_Sb.Append(e.OldFullPath);
                m_Sb.Append(" ");
                m_Sb.Append(e.ChangeType.ToString());
                m_Sb.Append(" ");
                m_Sb.Append("to ");
                m_Sb.Append(e.Name);
                m_Sb.Append("    ");
                m_Sb.Append(DateTime.Now.ToString());
                //updateFolder(e);
                m_bDirty = true;
                if (rdbFile.Checked)
                {
                    m_Watcher.Filter = e.Name;
                    m_Watcher.Path = e.FullPath.Substring(0, e.FullPath.Length - m_Watcher.Filter.Length);
                }
            }
        }

        private void tmrEditNotify_Tick(object sender, EventArgs e)
        {
            if (m_bDirty)
            {
                listNotification.BeginUpdate();
                listNotification.Items.Add(m_Sb.ToString());
                listNotification.EndUpdate();
                m_bDirty = false;
            }
        }

        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            if (rdbDir.Checked)
            {
                DialogResult resDialog = dlgOpenDir.ShowDialog();
                if (resDialog.ToString() == "OK")
                {
                    txtFile.Text = dlgOpenDir.SelectedPath;
                    //listBox1.Items.Add(dlgOpenDir.SelectedPath);
                }
            }
            else
            {
                DialogResult resDialog = dlgOpenFile.ShowDialog();
                if (resDialog.ToString() == "OK")
                {
                    txtFile.Text = dlgOpenFile.FileName;
                    //listBox1.Items.Add(dlgOpenFile.FileName);
                }
            }
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            DialogResult resDialog = dlgSaveFile.ShowDialog();
            if (resDialog.ToString() == "OK")
            {
                FileInfo fi = new FileInfo(dlgSaveFile.FileName);
                StreamWriter sw = fi.CreateText();
                foreach (string sItem in listNotification.Items)
                {
                    sw.WriteLine(sItem);
                }
                sw.Close();
            }
        }

        private void rdbFile_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbFile.Checked == true)
            {
                chkSubFolder.Enabled = false;
                chkSubFolder.Checked = false;
            }
        }

        private void rdbDir_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbDir.Checked == true)
            {
                chkSubFolder.Enabled = true;
            }
        }

 
        /*
        private void listbox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }
         */
        /*
        private void listbox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            int i;
            for (i = 0; i < s.Length; i++)
                listBox1.Items.Add(s[i]);
        }
         * */
                
        private void txtFile_TextChanged(object sender, DragEventArgs e)
        {

        }

        private void clrearbtn_Click(object sender, EventArgs e)
        {

            //MessageBox.Show(m_Sb.Length.ToString());
            for (int n = listNotification.Items.Count - 1; n >= 0; --n)
            {
                string removeitem = "";
                if (listNotification.Items[n].ToString().Contains(removeitem))
                {
                    listNotification.Items.RemoveAt(n);
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtFile_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

    }
}