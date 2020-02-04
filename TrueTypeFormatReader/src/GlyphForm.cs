using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TrueTypeFormatReader
{
	public partial class GlyphForm : Form
	{
		private float ColorTime = 0.0f;
		Color[] Colors = new Color[] { Color.Black, Color.Red, Color.Green, Color.Blue, Color.White };
		uint ColorIndex = 0;

		Bitmap bm;
		PictureBox picCanvas;

		public GlyphForm()
		{
			InitializeComponent();

			picCanvas = new PictureBox();
			picCanvas.Size = new Size(ClientSize.Width, ClientSize.Height);
			bm = new Bitmap(picCanvas.Width, picCanvas.Height);
			picCanvas.Image = null;
			Controls.Add(picCanvas);

			this.Resize += new EventHandler(ResizeFormEvent);
		}

		public void Update(float deltaTime)
		{
			ColorTime += deltaTime;
			if (ColorTime >= 500.0)
			{
				ColorTime -= 500;
				ColorIndex++;
				if (ColorIndex >= Colors.Length)
				{
					ColorIndex = 0;
				}
			}
		}

		public void Draw()
		{
			if (picCanvas.Image == null)
			{
				picCanvas.Image = CreateCanvasBitmap(picCanvas.Width, picCanvas.Height);
			}
			Graphics g = Graphics.FromImage(picCanvas?.Image);
			g.Clear(Colors[ColorIndex]);
			picCanvas.Invalidate();
		}

		public void ResizeFormEvent(object sender, EventArgs e)
		{
			picCanvas.Size = new Size(ClientSize.Width, ClientSize.Height);
			if(picCanvas.Width >0 && picCanvas.Height > 0)
			{
				picCanvas.Image = new Bitmap(picCanvas.Width, picCanvas.Height);
			}
			else
			{
				picCanvas.Image = null;
			}
			//bm = new Bitmap(picCanvas.Width, picCanvas.Height);
		}

		private Bitmap CreateCanvasBitmap(int width, int height)
		{
			if (Width > 0 && Height > 0)
			{
				return new Bitmap(Width, Height);
			}
			else
			{
				return null;
			}
		}
	}
}
