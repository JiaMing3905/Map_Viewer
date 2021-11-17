
namespace WinFormsApp1
{
    partial class MapView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // MapView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.DoubleBuffered = true;
            this.Name = "MapView";
            this.Size = new System.Drawing.Size(512, 512);
            this.Load += new System.EventHandler(this.MapView_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MapView_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MapView_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MapView_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MapView_MouseUp);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.MapView_MouseWheel);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
    }
}
