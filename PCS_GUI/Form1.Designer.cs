namespace PCS_GUI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dgvAlarms = new DataGridView();
            txtConnection = new TextBox();
            btnConnect = new Button();
            btnDisconnect = new Button();
            btnAcknowledge = new Button();
            btnDismiss = new Button();
            txtResponse = new TextBox();
            btnArchiveAll = new Button();
            btnArchive = new Button();
            label5 = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvAlarms).BeginInit();
            SuspendLayout();
            // 
            // dgvAlarms
            // 
            dgvAlarms.AllowUserToAddRows = false;
            dgvAlarms.AllowUserToDeleteRows = false;
            dgvAlarms.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAlarms.Location = new Point(319, 16);
            dgvAlarms.Margin = new Padding(3, 2, 3, 2);
            dgvAlarms.Name = "dgvAlarms";
            dgvAlarms.RowHeadersWidth = 51;
            dgvAlarms.Size = new Size(530, 196);
            dgvAlarms.TabIndex = 1;
            dgvAlarms.RowHeaderMouseClick += dgvAlarms_RowHeaderMouseClick;
            // 
            // txtConnection
            // 
            txtConnection.Location = new Point(26, 81);
            txtConnection.Margin = new Padding(3, 2, 3, 2);
            txtConnection.Multiline = true;
            txtConnection.Name = "txtConnection";
            txtConnection.Size = new Size(249, 65);
            txtConnection.TabIndex = 2;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(26, 16);
            btnConnect.Margin = new Padding(3, 2, 3, 2);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(109, 49);
            btnConnect.TabIndex = 7;
            btnConnect.Text = "Connect to Raspberry Pi";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // btnDisconnect
            // 
            btnDisconnect.Location = new Point(160, 16);
            btnDisconnect.Margin = new Padding(3, 2, 3, 2);
            btnDisconnect.Name = "btnDisconnect";
            btnDisconnect.Size = new Size(115, 49);
            btnDisconnect.TabIndex = 8;
            btnDisconnect.Text = "Disconnect from Raspberry Pi";
            btnDisconnect.UseVisualStyleBackColor = true;
            btnDisconnect.Click += btnDisconnect_Click;
            // 
            // btnAcknowledge
            // 
            btnAcknowledge.Location = new Point(319, 237);
            btnAcknowledge.Margin = new Padding(3, 2, 3, 2);
            btnAcknowledge.Name = "btnAcknowledge";
            btnAcknowledge.Size = new Size(148, 68);
            btnAcknowledge.TabIndex = 9;
            btnAcknowledge.Text = "Acknowledge alarm";
            btnAcknowledge.UseVisualStyleBackColor = true;
            btnAcknowledge.Click += btnAcknowledge_Click;
            // 
            // btnDismiss
            // 
            btnDismiss.Location = new Point(532, 237);
            btnDismiss.Margin = new Padding(3, 2, 3, 2);
            btnDismiss.Name = "btnDismiss";
            btnDismiss.Size = new Size(149, 68);
            btnDismiss.TabIndex = 10;
            btnDismiss.Text = "Dismiss alarm";
            btnDismiss.UseVisualStyleBackColor = true;
            btnDismiss.Click += btnDismiss_Click;
            // 
            // txtResponse
            // 
            txtResponse.Location = new Point(319, 322);
            txtResponse.Margin = new Padding(3, 2, 3, 2);
            txtResponse.Name = "txtResponse";
            txtResponse.ReadOnly = true;
            txtResponse.Size = new Size(218, 23);
            txtResponse.TabIndex = 13;
            // 
            // btnArchiveAll
            // 
            btnArchiveAll.Location = new Point(723, 301);
            btnArchiveAll.Margin = new Padding(3, 2, 3, 2);
            btnArchiveAll.Name = "btnArchiveAll";
            btnArchiveAll.Size = new Size(126, 41);
            btnArchiveAll.TabIndex = 14;
            btnArchiveAll.Text = "Archive all dismissed alarms";
            btnArchiveAll.UseVisualStyleBackColor = true;
            btnArchiveAll.Click += btnArchiveAll_Click;
            // 
            // btnArchive
            // 
            btnArchive.Location = new Point(723, 237);
            btnArchive.Margin = new Padding(3, 2, 3, 2);
            btnArchive.Name = "btnArchive";
            btnArchive.Size = new Size(126, 45);
            btnArchive.TabIndex = 15;
            btnArchive.Text = "Archive alarm";
            btnArchive.UseVisualStyleBackColor = true;
            btnArchive.Click += btnArchive_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(870, 230);
            label5.Name = "label5";
            label5.Size = new Size(139, 75);
            label5.TabIndex = 16;
            label5.Text = "Importance levels:\r\n2 = Active alarm\r\n1 = Acknowledged alarm\r\n0 = Dismissed alarm\r\n\r\n";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1125, 360);
            Controls.Add(label5);
            Controls.Add(btnArchive);
            Controls.Add(btnArchiveAll);
            Controls.Add(txtResponse);
            Controls.Add(btnDismiss);
            Controls.Add(btnAcknowledge);
            Controls.Add(btnDisconnect);
            Controls.Add(btnConnect);
            Controls.Add(txtConnection);
            Controls.Add(dgvAlarms);
            Margin = new Padding(3, 2, 3, 2);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)dgvAlarms).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private DataGridView dgvAlarms;
        private TextBox txtConnection;
        private TextBox textBox2;
        private TextBox textBox3;
        private Label label2;
        private Label label3;
        private Button btnConnect;
        private Button btnDisconnect;
        private Button btnAcknowledge;
        private Button btnDismiss;
        private TextBox textBox4;
        private Label label4;
        private TextBox txtResponse;
        private Button btnArchiveAll;
        private Button btnArchive;
        private Label label5;
    }
}
