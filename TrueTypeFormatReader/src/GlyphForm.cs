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
		uint GlyphIndex = 36;

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

			FontScale = 0.05f;//64.0f / TrueFont.UnitsPerEm; //64 is Font size, hard coded for now
		}

		public void Update(float deltaTime)
		{
			GlyphTime += deltaTime;
			if (GlyphTime >= 500.0)
			{
				GlyphTime -= 500 * (int)GlyphTime/500;
				GlyphIndex++;
				if (GlyphIndex >= TrueFont.Length)
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

		public void DrawGlyph(int xDrawPos, int yDrawPos)
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
			g.Clear(Color.HotPink);

			System.Drawing.Drawing2D.GraphicsPath fullPath = new System.Drawing.Drawing2D.GraphicsPath();

			int p=0, c= 0, first = 1;
			float firstX = 0, firstY = 0;
			float xOffset =0, yOffset = 0;
			bool offsetsSet = false;
			while(p < glyph.Points.Length)
			{
				if(first == 1)
				{
					first = 0;
					firstX = (glyph.Points[p].X * FontScale + FontScale*(-TrueFont.xMin));
					firstY = (glyph.Points[p].Y * -FontScale + FontScale * TrueFont.yMax);
					if(offsetsSet == false)
					{
						offsetsSet = true;
						xOffset = xDrawPos - firstX;
						yOffset = yDrawPos - FontScale * (TrueFont.yMax - TrueFont.yMin);
					}
					firstX += xOffset;
					firstY += yOffset;
					fullPath.StartFigure();
				}
				else
				{
					float X = (glyph.Points[p].X * FontScale + FontScale * (-TrueFont.xMin)) + xOffset;
					float Y = (glyph.Points[p].Y * -FontScale + FontScale * TrueFont.yMax) + yOffset;
					//g.DrawLine(new Pen(Color.White, 1), firstX - 200, firstY, X - 200, Y); used for manually drawing glyphs, but does not fill them

					fullPath.AddLine(firstX, firstY, X, Y);

					firstX = X;
					firstY = Y;					
				}

				if(p == glyph.ContourEnds[c])
				{
					c++;
					first = 1;
					fullPath.CloseFigure();
				}
				p++;
			}

			g.FillPath(new SolidBrush(Color.Black), fullPath);
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
