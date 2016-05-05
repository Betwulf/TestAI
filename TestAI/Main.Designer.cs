namespace TestAI
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.txtOut = new System.Windows.Forms.TextBox();
            this.btnMktData = new System.Windows.Forms.Button();
            this.btnGetUniverse = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnCalcData = new System.Windows.Forms.Button();
            this.btnCallML = new System.Windows.Forms.Button();
            this.btnTraining = new System.Windows.Forms.Button();
            this.btnClearCalcData = new System.Windows.Forms.Button();
            this.btnSimulate = new System.Windows.Forms.Button();
            this.dteCallAIStartDate = new System.Windows.Forms.DateTimePicker();
            this.btnGetMorehistory = new System.Windows.Forms.Button();
            this.dteCallAIEndDate = new System.Windows.Forms.DateTimePicker();
            this.btnAIRefreshData = new System.Windows.Forms.Button();
            this.btnDaily = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtOut
            // 
            this.txtOut.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOut.Location = new System.Drawing.Point(12, 148);
            this.txtOut.Multiline = true;
            this.txtOut.Name = "txtOut";
            this.txtOut.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOut.Size = new System.Drawing.Size(535, 508);
            this.txtOut.TabIndex = 1;
            // 
            // btnMktData
            // 
            this.btnMktData.Location = new System.Drawing.Point(109, 25);
            this.btnMktData.Name = "btnMktData";
            this.btnMktData.Size = new System.Drawing.Size(91, 23);
            this.btnMktData.TabIndex = 2;
            this.btnMktData.Text = "Fetch Mkt Data";
            this.btnMktData.UseVisualStyleBackColor = true;
            this.btnMktData.Click += new System.EventHandler(this.btnMktData_Click);
            // 
            // btnGetUniverse
            // 
            this.btnGetUniverse.Location = new System.Drawing.Point(12, 25);
            this.btnGetUniverse.Name = "btnGetUniverse";
            this.btnGetUniverse.Size = new System.Drawing.Size(91, 23);
            this.btnGetUniverse.TabIndex = 3;
            this.btnGetUniverse.Text = "Get Universe";
            this.btnGetUniverse.UseVisualStyleBackColor = true;
            this.btnGetUniverse.Click += new System.EventHandler(this.btnGetUniverse_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(12, 54);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(91, 23);
            this.btnClear.TabIndex = 4;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnCalcData
            // 
            this.btnCalcData.Location = new System.Drawing.Point(206, 25);
            this.btnCalcData.Name = "btnCalcData";
            this.btnCalcData.Size = new System.Drawing.Size(91, 23);
            this.btnCalcData.TabIndex = 5;
            this.btnCalcData.Text = "Calc Data";
            this.btnCalcData.UseVisualStyleBackColor = true;
            this.btnCalcData.Click += new System.EventHandler(this.btnCalcData_Click);
            // 
            // btnCallML
            // 
            this.btnCallML.Location = new System.Drawing.Point(303, 25);
            this.btnCallML.Name = "btnCallML";
            this.btnCallML.Size = new System.Drawing.Size(91, 23);
            this.btnCallML.TabIndex = 6;
            this.btnCallML.Text = "Call ML";
            this.btnCallML.UseVisualStyleBackColor = true;
            this.btnCallML.Click += new System.EventHandler(this.btnCallML_Click);
            // 
            // btnTraining
            // 
            this.btnTraining.Location = new System.Drawing.Point(206, 54);
            this.btnTraining.Name = "btnTraining";
            this.btnTraining.Size = new System.Drawing.Size(91, 23);
            this.btnTraining.TabIndex = 7;
            this.btnTraining.Text = "Calc Training";
            this.btnTraining.UseVisualStyleBackColor = true;
            this.btnTraining.Click += new System.EventHandler(this.btnTraining_Click);
            // 
            // btnClearCalcData
            // 
            this.btnClearCalcData.Location = new System.Drawing.Point(206, 83);
            this.btnClearCalcData.Name = "btnClearCalcData";
            this.btnClearCalcData.Size = new System.Drawing.Size(91, 23);
            this.btnClearCalcData.TabIndex = 8;
            this.btnClearCalcData.Text = "Clear Calc Data";
            this.btnClearCalcData.UseVisualStyleBackColor = true;
            this.btnClearCalcData.Click += new System.EventHandler(this.btnClearCalcData_Click);
            // 
            // btnSimulate
            // 
            this.btnSimulate.Location = new System.Drawing.Point(400, 25);
            this.btnSimulate.Name = "btnSimulate";
            this.btnSimulate.Size = new System.Drawing.Size(91, 23);
            this.btnSimulate.TabIndex = 9;
            this.btnSimulate.Text = "Simulate";
            this.btnSimulate.UseVisualStyleBackColor = true;
            this.btnSimulate.Click += new System.EventHandler(this.btnSimulate_Click);
            // 
            // dteCallAIStartDate
            // 
            this.dteCallAIStartDate.CustomFormat = "yyyy.MM.dd";
            this.dteCallAIStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dteCallAIStartDate.Location = new System.Drawing.Point(303, 83);
            this.dteCallAIStartDate.MaxDate = new System.DateTime(2019, 12, 31, 0, 0, 0, 0);
            this.dteCallAIStartDate.Name = "dteCallAIStartDate";
            this.dteCallAIStartDate.Size = new System.Drawing.Size(91, 20);
            this.dteCallAIStartDate.TabIndex = 10;
            this.dteCallAIStartDate.Value = new System.DateTime(2006, 7, 5, 0, 0, 0, 0);
            // 
            // btnGetMorehistory
            // 
            this.btnGetMorehistory.Location = new System.Drawing.Point(109, 54);
            this.btnGetMorehistory.Name = "btnGetMorehistory";
            this.btnGetMorehistory.Size = new System.Drawing.Size(91, 23);
            this.btnGetMorehistory.TabIndex = 11;
            this.btnGetMorehistory.Text = "Get History ";
            this.btnGetMorehistory.UseVisualStyleBackColor = true;
            this.btnGetMorehistory.Click += new System.EventHandler(this.btnGetMorehistory_Click);
            // 
            // dteCallAIEndDate
            // 
            this.dteCallAIEndDate.CustomFormat = "yyyy.MM.dd";
            this.dteCallAIEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dteCallAIEndDate.Location = new System.Drawing.Point(400, 83);
            this.dteCallAIEndDate.MaxDate = new System.DateTime(2019, 12, 31, 0, 0, 0, 0);
            this.dteCallAIEndDate.Name = "dteCallAIEndDate";
            this.dteCallAIEndDate.Size = new System.Drawing.Size(91, 20);
            this.dteCallAIEndDate.TabIndex = 12;
            this.dteCallAIEndDate.Value = new System.DateTime(2016, 4, 19, 0, 0, 0, 0);
            // 
            // btnAIRefreshData
            // 
            this.btnAIRefreshData.Location = new System.Drawing.Point(303, 54);
            this.btnAIRefreshData.Name = "btnAIRefreshData";
            this.btnAIRefreshData.Size = new System.Drawing.Size(91, 23);
            this.btnAIRefreshData.TabIndex = 13;
            this.btnAIRefreshData.Text = "Refresh ML";
            this.btnAIRefreshData.UseVisualStyleBackColor = true;
            this.btnAIRefreshData.Click += new System.EventHandler(this.btnAIRefresh_Click);
            // 
            // btnDaily
            // 
            this.btnDaily.BackColor = System.Drawing.SystemColors.Control;
            this.btnDaily.Location = new System.Drawing.Point(400, 109);
            this.btnDaily.Name = "btnDaily";
            this.btnDaily.Size = new System.Drawing.Size(91, 23);
            this.btnDaily.TabIndex = 14;
            this.btnDaily.Text = "Daily Update";
            this.btnDaily.UseVisualStyleBackColor = false;
            this.btnDaily.Click += new System.EventHandler(this.btnDaily_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(559, 668);
            this.Controls.Add(this.btnDaily);
            this.Controls.Add(this.btnAIRefreshData);
            this.Controls.Add(this.dteCallAIEndDate);
            this.Controls.Add(this.btnGetMorehistory);
            this.Controls.Add(this.dteCallAIStartDate);
            this.Controls.Add(this.btnSimulate);
            this.Controls.Add(this.btnClearCalcData);
            this.Controls.Add(this.btnTraining);
            this.Controls.Add(this.btnCallML);
            this.Controls.Add(this.btnCalcData);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnGetUniverse);
            this.Controls.Add(this.btnMktData);
            this.Controls.Add(this.txtOut);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.Text = "Test AI";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtOut;
        private System.Windows.Forms.Button btnMktData;
        private System.Windows.Forms.Button btnGetUniverse;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnCalcData;
        private System.Windows.Forms.Button btnCallML;
        private System.Windows.Forms.Button btnTraining;
        private System.Windows.Forms.Button btnClearCalcData;
        private System.Windows.Forms.Button btnSimulate;
        private System.Windows.Forms.DateTimePicker dteCallAIStartDate;
        private System.Windows.Forms.Button btnGetMorehistory;
        private System.Windows.Forms.DateTimePicker dteCallAIEndDate;
        private System.Windows.Forms.Button btnAIRefreshData;
        private System.Windows.Forms.Button btnDaily;
    }
}

