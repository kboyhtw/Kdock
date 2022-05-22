namespace Kdock
{
    partial class frmAccess
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Label1 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.Label4 = new System.Windows.Forms.Label();
            this.Label5 = new System.Windows.Forms.Label();
            this.btnYes = new System.Windows.Forms.Button();
            this.btnNo = new System.Windows.Forms.Button();
            this.lblMessege = new System.Windows.Forms.Label();
            this.tglGive = new Kdock.KToggleSwitch();
            this.tglGet = new Kdock.KToggleSwitch();
            this.tglCM = new Kdock.KToggleSwitch();
            this.tglDelete = new Kdock.KToggleSwitch();
            this.SuspendLayout();
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.Location = new System.Drawing.Point(34, 105);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(66, 21);
            this.Label1.TabIndex = 4;
            this.Label1.Text = "Delete :";
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.Location = new System.Drawing.Point(34, 64);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(82, 21);
            this.Label2.TabIndex = 5;
            this.Label2.Text = "User Can :";
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label3.Location = new System.Drawing.Point(231, 105);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(105, 21);
            this.Label3.TabIndex = 6;
            this.Label3.Text = "Copy/Move :";
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label4.Location = new System.Drawing.Point(34, 145);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(44, 21);
            this.Label4.TabIndex = 7;
            this.Label4.Text = "Get :";
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label5.Location = new System.Drawing.Point(231, 145);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(50, 21);
            this.Label5.TabIndex = 8;
            this.Label5.Text = "Give :";
            // 
            // btnYes
            // 
            this.btnYes.Location = new System.Drawing.Point(132, 196);
            this.btnYes.Name = "btnYes";
            this.btnYes.Size = new System.Drawing.Size(75, 23);
            this.btnYes.TabIndex = 9;
            this.btnYes.Text = "Yes";
            this.btnYes.UseVisualStyleBackColor = true;
            this.btnYes.Click += new System.EventHandler(this.btnYes_Click);
            // 
            // btnNo
            // 
            this.btnNo.Location = new System.Drawing.Point(238, 196);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(75, 23);
            this.btnNo.TabIndex = 10;
            this.btnNo.Text = "No";
            this.btnNo.UseVisualStyleBackColor = true;
            this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
            // 
            // lblMessege
            // 
            this.lblMessege.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessege.Location = new System.Drawing.Point(12, 9);
            this.lblMessege.Name = "lblMessege";
            this.lblMessege.Size = new System.Drawing.Size(421, 55);
            this.lblMessege.TabIndex = 11;
            this.lblMessege.Text = "msg";
            this.lblMessege.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tglGive
            // 
            this.tglGive.Location = new System.Drawing.Point(336, 140);
            this.tglGive.Name = "tglGive";
            this.tglGive.Size = new System.Drawing.Size(84, 26);
            this.tglGive.SwitchBackColor = System.Drawing.Color.DarkGreen;
            this.tglGive.SwitchForeColor = System.Drawing.Color.White;
            this.tglGive.TabIndex = 3;
            this.tglGive.Toggled = true;
            // 
            // tglGet
            // 
            this.tglGet.Location = new System.Drawing.Point(123, 140);
            this.tglGet.Name = "tglGet";
            this.tglGet.Size = new System.Drawing.Size(84, 26);
            this.tglGet.SwitchBackColor = System.Drawing.Color.DarkGreen;
            this.tglGet.SwitchForeColor = System.Drawing.Color.White;
            this.tglGet.TabIndex = 2;
            this.tglGet.Toggled = true;
            // 
            // tglCM
            // 
            this.tglCM.Location = new System.Drawing.Point(336, 100);
            this.tglCM.Name = "tglCM";
            this.tglCM.Size = new System.Drawing.Size(84, 26);
            this.tglCM.SwitchBackColor = System.Drawing.Color.DarkGreen;
            this.tglCM.SwitchForeColor = System.Drawing.Color.White;
            this.tglCM.TabIndex = 1;
            this.tglCM.Toggled = true;
            // 
            // tglDelete
            // 
            this.tglDelete.Location = new System.Drawing.Point(123, 100);
            this.tglDelete.Name = "tglDelete";
            this.tglDelete.Size = new System.Drawing.Size(84, 26);
            this.tglDelete.SwitchBackColor = System.Drawing.Color.DarkGreen;
            this.tglDelete.SwitchForeColor = System.Drawing.Color.White;
            this.tglDelete.TabIndex = 0;
            this.tglDelete.Toggled = true;
            // 
            // frmAccess
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(445, 231);
            this.Controls.Add(this.lblMessege);
            this.Controls.Add(this.btnNo);
            this.Controls.Add(this.btnYes);
            this.Controls.Add(this.Label5);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.tglGive);
            this.Controls.Add(this.tglGet);
            this.Controls.Add(this.tglCM);
            this.Controls.Add(this.tglDelete);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmAccess";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "File Explorer Request";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private KToggleSwitch tglDelete;

        private KToggleSwitch tglCM;

        private KToggleSwitch tglGet;

        private KToggleSwitch tglGive;

        private System.Windows.Forms.Label Label1;

        private System.Windows.Forms.Label Label2;

        private System.Windows.Forms.Label Label3;

        private System.Windows.Forms.Label Label4;

        private System.Windows.Forms.Label Label5;

        private System.Windows.Forms.Button btnYes;

        private System.Windows.Forms.Button btnNo;

        private System.Windows.Forms.Label lblMessege;

        #endregion
    }
}