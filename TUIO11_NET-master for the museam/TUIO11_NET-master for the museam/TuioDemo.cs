/*
	TUIO C# Demo - part of the reacTIVision project
	Copyright (c) 2005-2016 Martin Kaltenbrunner <martin@tuio.org>

	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using TUIO;
using System.Drawing.Imaging;
using static System.Windows.Forms.DataFormats;
using System.Diagnostics;

public class Truck
{
    public int X, Y;
    public Bitmap img;
    public int flagAttached = 0;
    public int dy = 1;

}
public class TuioDemo : Form , TuioListener
	{
    bool applicationOpened = false;
    Thread mThread;
    public string connectionIP = "127.0.0.1";
    public int connectionPort = 65434;
   // IPAddress localAdd;
    //TcpListener listener;
    //TcpClient client;
    bool running;

    List<Truck> T = new List<Truck>();

    private TuioClient client;
		private Dictionary<long,TuioObject> objectList;
		private Dictionary<long,TuioCursor> cursorList;
		private Dictionary<long,TuioBlob> blobList;

    private Form imageForm1;
    private Form imageForm2;
    private Form imageForm3;
    private Form imageForm4;


    public static int width, height;
		private int window_width =  640;
		private int window_height = 480;
		private int window_left = 0;
		private int window_top = 0;
		private int screen_width = Screen.PrimaryScreen.Bounds.Width;
		private int screen_height = Screen.PrimaryScreen.Bounds.Height;

		private bool fullscreen;
		private bool verbose;

		Font font = new Font("Arial", 10.0f);
		SolidBrush fntBrush = new SolidBrush(Color.White);
		SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(0,0,64));
		SolidBrush curBrush = new SolidBrush(Color.FromArgb(192, 0, 192));
		SolidBrush objBrush = new SolidBrush(Color.FromArgb(64, 0, 0));
		SolidBrush blbBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
		Pen curPen = new Pen(new SolidBrush(Color.Blue), 1);

		public TuioDemo(int port) {
		
			verbose = false;
			fullscreen = false;
			width = window_width;
			height = window_height;

			this.ClientSize = new System.Drawing.Size(width, height);
			this.Name = "TuioDemo";
			this.Text = "TuioDemo";
			
			this.Closing+=new CancelEventHandler(Form_Closing);
			this.KeyDown+=new KeyEventHandler(Form_KeyDown);

			this.SetStyle( ControlStyles.AllPaintingInWmPaint |
							ControlStyles.UserPaint |
							ControlStyles.DoubleBuffer, true);

			objectList = new Dictionary<long,TuioObject>(128);
			cursorList = new Dictionary<long,TuioCursor>(128);
			blobList   = new Dictionary<long,TuioBlob>(128);
			
			client = new TuioClient(port);
			client.addTuioListener(this);

			client.connect();
		}

		private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {

 			if ( e.KeyData == Keys.F1) {
	 			if (fullscreen == false) {

					width = screen_width;
					height = screen_height;

					window_left = this.Left;
					window_top = this.Top;

					this.FormBorderStyle = FormBorderStyle.None;
		 			this.Left = 0;
		 			this.Top = 0;
		 			this.Width = screen_width;
		 			this.Height = screen_height;

		 			fullscreen = true;
	 			} else {

					width = window_width;
					height = window_height;

		 			this.FormBorderStyle = FormBorderStyle.Sizable;
		 			this.Left = window_left;
		 			this.Top = window_top;
		 			this.Width = window_width;
		 			this.Height = window_height;

		 			fullscreen = false;
	 			}
 			} else if ( e.KeyData == Keys.Escape) {
				this.Close();

 			} else if ( e.KeyData == Keys.V ) {
 				verbose=!verbose;
 			}

 		}

		private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			client.removeTuioListener(this);

			client.disconnect();
			System.Environment.Exit(0);
		}

		public void addTuioObject(TuioObject o) {
			lock(objectList) {
				objectList.Add(o.SessionID,o);
			} if (verbose) Console.WriteLine("add obj "+o.SymbolID+" ("+o.SessionID+") "+o.X+" "+o.Y+" "+o.Angle);
		}

		public void updateTuioObject(TuioObject o) {

		
		if (verbose) Console.WriteLine("set obj "+o.SymbolID+" "+o.SessionID+" "+o.X+" "+o.Y+" "+o.Angle+" "+o.MotionSpeed+" "+o.RotationSpeed+" "+o.MotionAccel+" "+o.RotationAccel);

		//if (o.SymbolID == 1&& o.Angle ==50)
		//{
		//	create_form1();


		//	if (  (o.Angle > 87 && o.Angle < 100) || (o.Angle > 265 && o.Angle < 280) )// Rotate right threshold
		//	{
		//		this.Close();
		//	}
		//}

		if (o.SymbolID == 1)
		{
			OpenTaskForm(1, o.AngleDegrees);
			objectList.Remove(o.SessionID);
			refresh(new TuioTime());
			closeTaskForm(1, o.AngleDegrees);


		}
            //else
            //{
            //	if (imageForm1 == null || !imageForm1.Visible)
            //	{
            //              create_form1();
            //          }
            //      }

            if (o.SymbolID == 2)
        {
            create_form2();
        }

        if (o.SymbolID == 3)
        {
            create_form3();
        }

    }


	public void OpenTaskForm(int ID, float Angle)
	{
		if (ID == 1 && (Angle > 87 && Angle < 100) || (Angle > 265 && Angle < 280))
		{
            Bitmap bitmap = new Bitmap("1.jpg");

            Form imageForm = new Form();
            imageForm.Text = "Image Form";
            imageForm.StartPosition = FormStartPosition.CenterScreen; // Center the form on the screen

            PictureBox pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Top; // Dock the PictureBox to the top of the form
            pictureBox.Image = bitmap;
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom; // Ensure the image fits within the picture box
            pictureBox.Width = bitmap.Width + 90; // Double the width of the PictureBox
            pictureBox.Height = bitmap.Height + 90;
            imageForm.Controls.Add(pictureBox);

            // Add a Label control for the paragraph of text
            Label paragraphLabel = new Label();
            paragraphLabel.Text = "Your paragraph of text goes here.";
            paragraphLabel.Dock = DockStyle.Fill; // Dock the Label to fill the remaining space below the PictureBox
            paragraphLabel.TextAlign = ContentAlignment.MiddleCenter; // Center the text horizontally
            imageForm.Controls.Add(paragraphLabel);

            // Set the size of the form to accommodate the image and text
            imageForm.ClientSize = new Size(bitmap.Width, bitmap.Height + paragraphLabel.PreferredHeight);
            //MessageBox.Show("Tutankhamun was nine years of age when he ascended to the throne after the death of King Akhenaten's coregent, Smenkhkare. Shortly after his coronation, Tutankhamun was married to Ankhesenpaaton, Akhenaten's third daughter and (probably) the eldest surviving princess of the royal family.");
            // Show the new form
            imageForm.ShowDialog();

            objectList.Remove(ID);
			Console.WriteLine(Angle.ToString());


			//if ( (Angle > 177 && Angle < 190) || (Angle > 0 && Angle < 15) || (Angle > 180 && Angle < 195))
			//{
			//	imageForm.Close();
			//}


		}


	}

    public void closeTaskForm(int ID, float Angle)
    {
        if (Angle >= 45 && Angle < 135)
        {
           
        
         MessageBox.Show("this is king tut");
            objectList.Remove(ID);
            Console.WriteLine(Angle.ToString());
        }
    }

    //public void closeTaskForm(int ID, float Angle)
    //{
    //    if (ID == 1 && (Angle < 87 && Angle >100) || (Angle < 265 && Angle > 280))
    //    {
    //        Bitmap bitmap = new Bitmap("1.jpg");
    //        Form imageForm = new Form();
    //        PictureBox pictureBox = new PictureBox();
    //        pictureBox.Dock = DockStyle.Top; // Dock the PictureBox to the top of the form
    //        pictureBox.Image = bitmap;
    //        pictureBox.SizeMode = PictureBoxSizeMode.Zoom; // Ensure the image fits within the picture box
    //        pictureBox.Width = bitmap.Width + 90; // Double the width of the PictureBox
    //        pictureBox.Height = bitmap.Height + 90;
    //        imageForm.Controls.Add(pictureBox);
    //        imageForm.Close();


    //        //Form TaskForm = new Form();
    //        //TaskForm.ShowDialog();
    //        objectList.Remove(ID);
    //        Console.WriteLine(Angle.ToString());

    //    }



    //}
    public void create_form1()
	{
        Bitmap bitmap = new Bitmap("1.jpg");


        Form imageForm = new Form();
        imageForm.Text = "Image Form";
        imageForm.StartPosition = FormStartPosition.CenterScreen; // Center the form on the screen

        PictureBox pictureBox = new PictureBox();
        pictureBox.Dock = DockStyle.Top; // Dock the PictureBox to the top of the form
        pictureBox.Image = bitmap;
        pictureBox.SizeMode = PictureBoxSizeMode.Zoom; // Ensure the image fits within the picture box
        pictureBox.Width = bitmap.Width + 90; // Double the width of the PictureBox
        pictureBox.Height = bitmap.Height + 90;
        imageForm.Controls.Add(pictureBox);

        // Add a Label control for the paragraph of text
        Label paragraphLabel = new Label();
        paragraphLabel.Text = "Your paragraph of text goes here.";
        paragraphLabel.Dock = DockStyle.Fill; // Dock the Label to fill the remaining space below the PictureBox
        paragraphLabel.TextAlign = ContentAlignment.MiddleCenter; // Center the text horizontally
        imageForm.Controls.Add(paragraphLabel);

        // Set the size of the form to accommodate the image and text
        imageForm.ClientSize = new Size(bitmap.Width, bitmap.Height + paragraphLabel.PreferredHeight);
        //MessageBox.Show("Tutankhamun was nine years of age when he ascended to the throne after the death of King Akhenaten's coregent, Smenkhkare. Shortly after his coronation, Tutankhamun was married to Ankhesenpaaton, Akhenaten's third daughter and (probably) the eldest surviving princess of the royal family.");
        // Show the new form
        imageForm.ShowDialog();

    }

    public void create_form2()
    {
        Bitmap bitmap = new Bitmap("2.jpeg");


        Form imageForm = new Form();
        imageForm.Text = "Image Form";
        imageForm.StartPosition = FormStartPosition.CenterScreen; // Center the form on the screen

        PictureBox pictureBox = new PictureBox();
        pictureBox.Dock = DockStyle.Top; // Dock the PictureBox to the top of the form
        pictureBox.Image = bitmap;
        pictureBox.SizeMode = PictureBoxSizeMode.Zoom; // Ensure the image fits within the picture box
        pictureBox.Width = bitmap.Width + 90; // Double the width of the PictureBox
        pictureBox.Height = bitmap.Height + 90;
        imageForm.Controls.Add(pictureBox);

        // Add a Label control for the paragraph of text
        Label paragraphLabel = new Label();
        paragraphLabel.Text = "Your paragraph of text goes here.";
        paragraphLabel.Dock = DockStyle.Fill; // Dock the Label to fill the remaining space below the PictureBox
        paragraphLabel.TextAlign = ContentAlignment.MiddleCenter; // Center the text horizontally
        imageForm.Controls.Add(paragraphLabel);

        // Set the size of the form to accommodate the image and text
        imageForm.ClientSize = new Size(bitmap.Width, bitmap.Height + paragraphLabel.PreferredHeight);
        //MessageBox.Show("Tutankhamun was nine years of age when he ascended to the throne after the death of King Akhenaten's coregent, Smenkhkare. Shortly after his coronation, Tutankhamun was married to Ankhesenpaaton, Akhenaten's third daughter and (probably) the eldest surviving princess of the royal family.");
        // Show the new form
        imageForm.ShowDialog();

    }


    public void create_form3()
    {
        Bitmap bitmap = new Bitmap("3.jpeg");


        Form imageForm = new Form();
        imageForm.Text = "Image Form";
        imageForm.StartPosition = FormStartPosition.CenterScreen; // Center the form on the screen

        PictureBox pictureBox = new PictureBox();
        pictureBox.Dock = DockStyle.Top; // Dock the PictureBox to the top of the form
        pictureBox.Image = bitmap;
        pictureBox.SizeMode = PictureBoxSizeMode.Zoom; // Ensure the image fits within the picture box
        pictureBox.Width = bitmap.Width + 90; // Double the width of the PictureBox
        pictureBox.Height = bitmap.Height + 90;
        imageForm.Controls.Add(pictureBox);

        // Add a Label control for the paragraph of text
        Label paragraphLabel = new Label();
        paragraphLabel.Text = "Your paragraph of text goes here.";
        paragraphLabel.Dock = DockStyle.Fill; // Dock the Label to fill the remaining space below the PictureBox
        paragraphLabel.TextAlign = ContentAlignment.MiddleCenter; // Center the text horizontally
        imageForm.Controls.Add(paragraphLabel);

        // Set the size of the form to accommodate the image and text
        imageForm.ClientSize = new Size(bitmap.Width, bitmap.Height + paragraphLabel.PreferredHeight);
        //MessageBox.Show("Tutankhamun was nine years of age when he ascended to the throne after the death of King Akhenaten's coregent, Smenkhkare. Shortly after his coronation, Tutankhamun was married to Ankhesenpaaton, Akhenaten's third daughter and (probably) the eldest surviving princess of the royal family.");
        // Show the new form
        imageForm.ShowDialog();

    }

    public void create_form4()
    {
        Bitmap bitmap = new Bitmap("4.jpeg");


        Form imageForm = new Form();
        imageForm.Text = "Image Form";
        imageForm.StartPosition = FormStartPosition.CenterScreen; // Center the form on the screen

        PictureBox pictureBox = new PictureBox();
        pictureBox.Dock = DockStyle.Top; // Dock the PictureBox to the top of the form
        pictureBox.Image = bitmap;
        pictureBox.SizeMode = PictureBoxSizeMode.Zoom; // Ensure the image fits within the picture box
        pictureBox.Width = bitmap.Width + 90; // Double the width of the PictureBox
        pictureBox.Height = bitmap.Height + 90;
        imageForm.Controls.Add(pictureBox);

        // Add a Label control for the paragraph of text
        Label paragraphLabel = new Label();
        paragraphLabel.Text = "Your paragraph of text goes here.";
        paragraphLabel.Dock = DockStyle.Fill; // Dock the Label to fill the remaining space below the PictureBox
        paragraphLabel.TextAlign = ContentAlignment.MiddleCenter; // Center the text horizontally
        imageForm.Controls.Add(paragraphLabel);

        // Set the size of the form to accommodate the image and text
        imageForm.ClientSize = new Size(bitmap.Width, bitmap.Height + paragraphLabel.PreferredHeight);
        //MessageBox.Show("Tutankhamun was nine years of age when he ascended to the throne after the death of King Akhenaten's coregent, Smenkhkare. Shortly after his coronation, Tutankhamun was married to Ankhesenpaaton, Akhenaten's third daughter and (probably) the eldest surviving princess of the royal family.");
        // Show the new form
        imageForm.ShowDialog();

    }


    public void removeTuioObject(TuioObject o) {
			lock(objectList) {
				objectList.Remove(o.SessionID);
			}
			if (verbose) Console.WriteLine("del obj "+o.SymbolID+" ("+o.SessionID+")");
		}

		public void addTuioCursor(TuioCursor c) {
			lock(cursorList) {
				cursorList.Add(c.SessionID,c);
			}
			if (verbose) Console.WriteLine("add cur "+c.CursorID + " ("+c.SessionID+") "+c.X+" "+c.Y);
		}

		public void updateTuioCursor(TuioCursor c) {
			if (verbose) Console.WriteLine("set cur "+c.CursorID + " ("+c.SessionID+") "+c.X+" "+c.Y+" "+c.MotionSpeed+" "+c.MotionAccel);
		}

		public void removeTuioCursor(TuioCursor c) {
			lock(cursorList) {
				cursorList.Remove(c.SessionID);
			}
			if (verbose) Console.WriteLine("del cur "+c.CursorID + " ("+c.SessionID+")");
 		}

		public void addTuioBlob(TuioBlob b) {
			lock(blobList) {
				blobList.Add(b.SessionID,b);
			}
			if (verbose) Console.WriteLine("add blb "+b.BlobID + " ("+b.SessionID+") "+b.X+" "+b.Y+" "+b.Angle+" "+b.Width+" "+b.Height+" "+b.Area);
		}

		public void updateTuioBlob(TuioBlob b) {
		
			if (verbose) Console.WriteLine("set blb "+b.BlobID + " ("+b.SessionID+") "+b.X+" "+b.Y+" "+b.Angle+" "+b.Width+" "+b.Height+" "+b.Area+" "+b.MotionSpeed+" "+b.RotationSpeed+" "+b.MotionAccel+" "+b.RotationAccel);
		}

		public void removeTuioBlob(TuioBlob b) {
			lock(blobList) {
				blobList.Remove(b.SessionID);
			}
			if (verbose) Console.WriteLine("del blb "+b.BlobID + " ("+b.SessionID+")");
		}

		public void refresh(TuioTime frameTime) {
			Invalidate();
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			// Getting the graphics object
			Graphics g = pevent.Graphics;
			g.FillRectangle(bgrBrush, new Rectangle(0,0,width,height));

			// draw the cursor path
			if (cursorList.Count > 0) {
 			 lock(cursorList) {
			 foreach (TuioCursor tcur in cursorList.Values) {
					List<TuioPoint> path = tcur.Path;
					TuioPoint current_point = path[0];

					for (int i = 0; i < path.Count; i++) {
						TuioPoint next_point = path[i];
						g.DrawLine(curPen, current_point.getScreenX(width), current_point.getScreenY(height), next_point.getScreenX(width), next_point.getScreenY(height));
						current_point = next_point;
					}
					g.FillEllipse(curBrush, current_point.getScreenX(width) - height / 100, current_point.getScreenY(height) - height / 100, height / 50, height / 50);
					g.DrawString(tcur.CursorID + "", font, fntBrush, new PointF(tcur.getScreenX(width) - 10, tcur.getScreenY(height) - 10));
				}
			}
		 }

			// draw the objects
			if (objectList.Count > 0) {
 				lock(objectList) {
					foreach (TuioObject tobj in objectList.Values) {
						int ox = tobj.getScreenX(width);
						int oy = tobj.getScreenY(height);
						int size = height / 10;

						g.TranslateTransform(ox, oy);
						g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
						g.TranslateTransform(-ox, -oy);

						g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));

						g.TranslateTransform(ox, oy);
						g.RotateTransform(-1 * (float)(tobj.Angle / Math.PI * 180.0f));
						g.TranslateTransform(-ox, -oy);

						g.DrawString(tobj.SymbolID + "", font, fntBrush, new PointF(ox - 10, oy - 10));
					}
				}
			}

			// draw the blobs
			if (blobList.Count > 0) {
				lock(blobList) {
					foreach (TuioBlob tblb in blobList.Values) {
						int bx = tblb.getScreenX(width);
						int by = tblb.getScreenY(height);
						float bw = tblb.Width*width;
						float bh = tblb.Height*height;

						g.TranslateTransform(bx, by);
						g.RotateTransform((float)(tblb.Angle / Math.PI * 180.0f));
						g.TranslateTransform(-bx, -by);

						g.FillEllipse(blbBrush, bx - bw / 2, by - bh / 2, bw, bh);

						g.TranslateTransform(bx, by);
						g.RotateTransform(-1 * (float)(tblb.Angle / Math.PI * 180.0f));
						g.TranslateTransform(-bx, -by);
						
						g.DrawString(tblb.BlobID + "", font, fntBrush, new PointF(bx, by));
					}
				}
			}
		}

		public static void Main(String[] argv) {
	 		int port = 0;
        bool applicationOpened = false;
        if (!applicationOpened)
        {
            string pathToFile1 = "D:\\browsers downloads\\CSharpClient\\CSharpClient\\CSharpClient\\bin\\Debug\\CSharpClient.exe";

            //tuio exe
            ProcessStartInfo startInfo1 = new ProcessStartInfo(pathToFile1);
            startInfo1.WindowStyle = ProcessWindowStyle.Minimized;
            System.Diagnostics.Process.Start(startInfo1);

            applicationOpened = true; // Set the flag when the process is opened
        }
        switch (argv.Length) {
				case 1:
					port = int.Parse(argv[0],null);
					if(port==0) goto default;
					break;
				case 0:
					port = 3333;
					break;
				default:
					Console.WriteLine("usage: mono TuioDemo [port]");
					System.Environment.Exit(0);
					break;
			}
			
			TuioDemo app = new TuioDemo(port);
			Application.Run(app);
		}
	}
