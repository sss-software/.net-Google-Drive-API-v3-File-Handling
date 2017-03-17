﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GDUploaderForm;

using System.Windows.Forms;

namespace GDUploaderForm
{
    public partial class frmMain : Form
    {
        public static string CredentialFolderName;

        public frmMain()
        {
            InitializeComponent();
            pnlDragAndDrop.AllowDrop = true;
            pnlDragAndDrop.DragEnter += new DragEventHandler(pnlDragAndDrop_DragEnter);
            pnlDragAndDrop.DragDrop += new DragEventHandler(pnlDragAndDrop_DragDrop);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            dgvFilesFromDrive.ColumnCount = 2;
            dgvFilesFromDrive.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFilesFromDrive.Columns[0].Name = "Name";
            dgvFilesFromDrive.Columns[1].Name = "ID";
            dgvFilesFromDrive.Font = new Font(FontFamily.GenericSansSerif, 9.0F, FontStyle.Bold);


            txtConnect.BackColor = Color.Red;
            txtConnect.Text = "Disconnected";
            txtAppName.Text = "Google Drive Uploader";
        }

        private void updateDataGridView()
        {
            dgvFilesFromDrive.Rows.Clear();
            foreach(string[] array in GoogleDriveAPIV3.updateDriveFiles())
            {
                dgvFilesFromDrive.Rows.Add(array);
            }
        }

        private void pnlDragAndDrop_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void pnlDragAndDrop_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                txtFilePath.Text = file;
                System.Diagnostics.Debug.WriteLine("File: {0}", file);
            } 
        }

        private void pnlConnection_DragDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void pnlConnection_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                txtJsonPath.Text = file;
                System.Diagnostics.Debug.WriteLine("File: {0}", file);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if(txtJsonPath.Text == string.Empty)
            {
                MessageBox.Show("You have to:" + Environment.NewLine +
                    "A) Select the client_secret.Json file "+ Environment.NewLine+
                    "B) Type your OAuth 2.0 client ID in Application Name TextBox"+Environment.NewLine+
                    "in order to begining connection with your Google Drive");
            }
            else
            {
                if (GoogleDriveAPIV3.GoogleDriveConnection(txtJsonPath.Text, txtAppName.Text))
                {
                    txtConnect.BackColor = Color.Green;
                    txtConnect.Text = "Connected";
                    updateDataGridView();
                }
                else
                {
                    txtConnect.BackColor = Color.Red;
                    txtConnect.Text = "Disconnected";
                }

            }
            
        }

        private void textBox_path_TextChanged(object sender, EventArgs e)
        {
            txtFileName.Text = Path.GetFileName(txtFilePath.Text);
        }

        private void btnDirToUpload_Click(object sender, EventArgs e)
        {
            DialogResult result = fbdDirToUpload.ShowDialog();
            switch (result)
            {
                case DialogResult.OK:
                    txtFilePath.Text = fbdDirToUpload.SelectedPath;
                    break;
                default:
                    break;
            }
        }

        private void button_browse_Click(object sender, EventArgs e)
        {
            DialogResult result = ofgFileToUpload.ShowDialog();
            switch (result)
            {
                case DialogResult.OK:
                    txtFilePath.Text = ofgFileToUpload.FileName;
                    break;
                default:
                    break;
            }
        }


        private void btnJsonBroswe_Click(object sender, EventArgs e)
        {
            DialogResult result = ofgJsonFile.ShowDialog();
            switch (result)
            {
                case DialogResult.OK:
                    txtJsonPath.Text = ofgJsonFile.FileName;
                    break;
                default:
                    break;
            }
        }
        private void downloadFile(string fileName, string fileID)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            switch (result)
            {
                case DialogResult.OK:
                    txtJsonPath.Text = ofgJsonFile.FileName;
                    GoogleDriveAPIV3.downloadFromDrive(fileName, fileID, fbd.SelectedPath);
                    break;
                default:
                    break;
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(txtFilePath.Text);
            string filePath = txtFilePath.Text;
            string fileName = txtFileName.Text;
            if(txtConnect.Text == "Disconnected")
            {
                MessageBox.Show("You have to Connect First in order to upload Files");
            }
            else
            {
                GoogleDriveAPIV3.uploadToDrive(filePath, fileName, null);
                updateDataGridView();
            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            string fileName, fileId;
            if (dgvFilesFromDrive.SelectedRows.Count <= 0)
            {
                MessageBox.Show("You have to select a row  in order to download");

            }
            else
            {
                foreach (DataGridViewRow row in dgvFilesFromDrive.SelectedRows)
                {
                    fileName = dgvFilesFromDrive.Rows[row.Index].Cells[0].Value.ToString();
                    fileId = dgvFilesFromDrive.Rows[row.Index].Cells[1].Value.ToString();
                    downloadFile(fileName, fileId);
                }
            }
        }

        private void dgvFilesFromDrive_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            string fileID = dgvFilesFromDrive.Rows[e.RowIndex].Cells[1].Value.ToString();
            string fileName = dgvFilesFromDrive.Rows[e.RowIndex].Cells[0].Value.ToString();
            downloadFile(fileName, fileID);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            string  fileId, fileName;
            if (dgvFilesFromDrive.SelectedRows.Count <= 0)
            {
                MessageBox.Show("You have to select a row  in order to download");

            }
            else
            {
                foreach (DataGridViewRow row in dgvFilesFromDrive.SelectedRows)
                {
                    fileName = dgvFilesFromDrive.Rows[row.Index].Cells[0].Value.ToString();
                    fileId = dgvFilesFromDrive.Rows[row.Index].Cells[1].Value.ToString();
                    DialogResult result = MessageBox.Show("Do you want to delete file: " + fileName, "Confirm",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    switch (result)
                    {
                        case DialogResult.Yes:
                            GoogleDriveAPIV3.removeFile(fileId);
                            updateDataGridView();
                            break;
                        default:
                            updateDataGridView();
                            break;
                    }
                }
            }
        }
    }
}
