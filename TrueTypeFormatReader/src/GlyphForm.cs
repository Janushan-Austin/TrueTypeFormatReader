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
		private float GlyphTime = 0.0f;
		uint GlyphIndex = 0;

		TrueTypeFont TrueFont;
		PictureBox picCanvas;

		float FontScale;

		public GlyphForm()
		{
			InitializeComponent();

			picCanvas = new PictureBox();
			picCanvas.Size = new Size(ClientSize.Width, ClientSize.Height);
			picCanvas.Image = null;
			Controls.Add(picCanvas);

			this.Resize += new EventHandler(ResizeFormEvent);
		}

		public GlyphForm(string fontFile)
		{
			InitializeComponent();

			picCanvas = new PictureBox();
			picCanvas.Size = new Size(ClientSize.Width, ClientSize.Height);
			picCanvas.Image = null;
			Controls.Add(picCanvas);

			this.Resize += new EventHandler(ResizeFormEvent);

			ReadFontFile(fontFile);
		}

		public void ReadFontFile(string fontFile)
		{
			System.IO.FileStream fileStream = System.IO.File.OpenRead(fontFile);

			byte[] byteBuffer = new byte[fileStream.Length];
			fileStream.Read(byteBuffer, 0, byteBuffer.Length);


			TrueFont = new TrueTypeFont(byteBuffer);
		}

		public void Update(float deltaTime)
		{
			GlyphTime += deltaTime;
			if (GlyphTime >= 500.0)
			{
				GlyphTime -= 500;
				GlyphIndex++;
				if (GlyphIndex >= 0)
				{
					GlyphIndex = 0;
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
			g.Clear(Color.Black);



			picCanvas.Invalidate();
		}

		public void DrawGlyph(int x, int y)
		{
			TrueTypeFont.Glyph glyph = TrueFont.ReadGlyph(GlyphIndex);

			if (glyph == null || glyph.Type != "simple")
			{
				return;
			}
			if (picCanvas.Image == null)
			{
				picCanvas.Image = CreateCanvasBitmap(picCanvas.Width, picCanvas.Height);
			}
			Graphics g = Graphics.FromImage(picCanvas?.Image);
			g.Clear(Color.Black);

			int p=0, c= 0, first = 1;
			int firstX = 0, firstY = 0;
			while(p < glyph.Points.Length)
			{
				if(first == 1)
				{
					first = 0;
					firstX = glyph.Points[p].X;
					firstY = glyph.Points[p].Y;
				}
				else
				{
					g.DrawLine(new Pen(Color.White), firstX, firstY, glyph.Points[p].X, glyph.Points[p].Y);
				}

				if(p == glyph.ContourEnds[c])
				{
					c++;
					first = 1;
				}
				p++;
			}


			g.DrawLine(new Pen(Color.HotPink), 0, 0, picCanvas.Width, picCanvas.Height);
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

		public void DrawGlyph(uint index)
		{

		}
	}
}
