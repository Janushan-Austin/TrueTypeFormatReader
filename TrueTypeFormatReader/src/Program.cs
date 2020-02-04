using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace TrueTypeFormatReader
{
    class Program
    {
        static void Main(string[] args)
        {
            // The code provided will print ‘Hello World’ to the console.
            // Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.
            //Console.WriteLine("Hello World!");
            //Console.ReadKey();

            System.IO.FileStream fileStream = System.IO.File.OpenRead("FontAwesome.ttf");

            byte[] byteBuffer = new byte[fileStream.Length];
            fileStream.Read(byteBuffer, 0, byteBuffer.Length);


            TrueTypeFont ttr = new TrueTypeFont(byteBuffer);
            System.Windows.Forms.Form form = new Form();
            Bitmap bm = new Bitmap(800, 600);
            PictureBox picCanvas = new PictureBox();
            picCanvas.Image = bm;
            form.CreateGraphics();
            form.Controls.Add(picCanvas);

            ttr.DrawGlyph(ref bm, 0);

            picCanvas.Image = bm;
            picCanvas.Refresh();
            form.Refresh();

            Application.Run(form);

            System.Console.Read();


            // Go to http://aka.ms/dotnet-get-started-console to continue learning how to build a console app! 
        }
    }


    public class TrueTypeFont
    {
        struct Table
        {
            public uint Checksum, Offset, Length;

            public Table(uint sum, uint offset, uint length)
            {
                Checksum = sum;
                Offset = offset;
                Length = length;
            }
        }

        struct Point
        {
            public bool OnCurve;
            public int X, Y;
            public Point(bool onCurve)
            {
                OnCurve = onCurve;
                X = Y = 0;
            }
        }

        class Glyph
        {
            public short NumberOfContours;
            public ushort xMin, yMin, xMax, yMax;

            public string Type;
            public ushort[] ContourEnds;
            public Point[] Points;

            public Glyph(short num, ushort xmin, ushort xmax, ushort ymin, ushort ymax)
            {
                NumberOfContours = num;
                xMin = xmin;
                yMin = ymin;
                xMax = xmax;
                yMax = ymax;

                Type = "";
                ContourEnds = null;
                Points = null;
            }
        }

        public BinaryReader File;
        private Dictionary<string, Table> Tables;

        public uint ScalerType, CheckSumAdjustment, MagicNumber;
        public ushort SearchRange, EntrySelector, RangeShift;

        ushort Flags, UnitsPerEm, xMin, yMin, xMax, yMax, MacStyle, LowestRecPPEM,
               FontDirectionHint, IndexToLocFormat, glyphDataFormat; 
        public ushort Length;

        Decimal Version, FontRevision;
        Int64 CreatedDate, ModifiedDate;

        public TrueTypeFont(byte[] buffer)
        {
            File = new BinaryReader(buffer);
            Tables = new Dictionary<string, Table>();
            ReadOffsetTables();
            ReadHeadTable();
            Length = GetGlyphCount();
        }

        private void ReadOffsetTables()
        {
            ScalerType = File.getUint32();
            ushort numTables = File.getUint16();
            SearchRange = File.getUint16();
            EntrySelector = File.getUint16();
            RangeShift = File.getUint16();

            for (ushort i = 0; i < numTables; i++)
            {
                string tag = File.getString(4);
                Table temp = new Table(File.getUint32(), File.getUint32(), File.getUint32());
                Tables.Add(tag, temp);

                if (!tag.Equals("head"))
                {
                    Debug.Assert(CalculateChecksum(temp.Offset, temp.Length) == temp.Checksum, "Table Checksum did not match Calculated Checksum");
                }
            }
        }

        private uint CalculateChecksum(uint offset, uint numberBytesInTable)
        {
            uint old = File.Seek(offset);
            uint sum = 0;
            uint nLongs = (numberBytesInTable + 3) / 4;
            while (nLongs > 0)
            {
                nLongs--;
                sum += File.getUint32();
            }
            File.Seek(old);
            return sum;
        }

        private void ReadHeadTable()
        {
            Debug.Assert(Tables.ContainsKey("head"), "head does not exist in True Type Tables");
            File.Seek(Tables["head"].Offset);
            Version = File.getFixed();
            FontRevision = File.getFixed();
            CheckSumAdjustment = File.getUint32();
            MagicNumber = File.getUint32();
            Debug.Assert(MagicNumber == 0x5F0F3CF5);
            Flags = File.getUint16();
            UnitsPerEm = File.getUint16();
            CreatedDate = File.getDate();
            ModifiedDate = File.getDate();
            xMin = File.getFword();
            yMin = File.getFword();
            xMax = File.getFword();
            yMax = File.getFword();
            MacStyle = File.getUint16();
            LowestRecPPEM = File.getUint16();
            FontDirectionHint = File.getUint16();
            IndexToLocFormat = File.getUint16();
            glyphDataFormat = File.getUint16();
        }

        private ushort GetGlyphCount()
        {
            uint old = File.Seek(Tables["maxp"].Offset + 4);
            ushort glyphCount = File.getUint16();
            File.Seek(old);
            return glyphCount;
        }

        private Glyph ReadGlyph(uint index)
        {
            uint offset = getGlyphOffset(index);

            if(offset < Tables["glyf"].Offset || offset >= Tables["glyf"].Offset + Tables["glyf"].Length)
            {
                return null;
            }

            Debug.Assert(offset >= this.Tables["glyf"].Offset);
            Debug.Assert(offset < this.Tables["glyf"].Offset + this.Tables["glyf"].Length);

            File.Seek(offset);

            Glyph glyph = new Glyph(File.getInt16(), File.getFword(), File.getFword(), File.getFword(), File.getFword());

            Debug.Assert(glyph.NumberOfContours >= -1);

            if (glyph.NumberOfContours == -1)
            {
                //readCompoundGlyph
            }
            else
            {
                ReadSimpleGlyph(ref glyph);
            }

            return glyph;
        }

        private void ReadSimpleGlyph(ref Glyph glyph)
        {
            byte ON_CURVE = 1,
            X_IS_BYTE = 2,
            Y_IS_BYTE = 4,
            REPEAT = 8,
            X_DELTA = 16,
            Y_DELTA = 32;


            glyph.Type = "simple";
            glyph.ContourEnds = new ushort[glyph.NumberOfContours];
            glyph.Points = new Point[glyph.NumberOfContours + 1];
            ref Point[] points = ref glyph.Points;
            byte[] flags = new byte[points.Length];

            for (var i = 0; i < glyph.NumberOfContours; i++)
            {
                glyph.ContourEnds[i] = File.getUint16();
            }

            // skip over intructions
            File.Seek(File.getUint16() + File.Tell());

            if (glyph.NumberOfContours == 0)
            {
                return;
            }

            for (int i = 0; i < points.Length; i++)
            {
                byte flag = File.getUint8();
                flags[i] = flag;
                points[i] = new Point(onCurve: (flag & ON_CURVE) > 0);

                if ((flag & REPEAT) > 0)
                {
                    byte repeatCount = File.getUint8();
                    Debug.Assert(repeatCount > 0);
                    i += repeatCount;
                    while (repeatCount > 0)
                    {
                        flags[i] = (flag);
                        points[i] = new Point(onCurve: (flag & ON_CURVE) > 0);
                        repeatCount--;
                    }
                }
            }
            ReadCoords(ref glyph, flags, "X", X_IS_BYTE, X_DELTA, glyph.xMin, glyph.xMax);
            ReadCoords(ref glyph, flags, "Y", Y_IS_BYTE, Y_DELTA, glyph.yMin, glyph.yMax);
        }

        private void ReadCoords(ref Glyph glyph, byte[] flags, string name, byte byteFlag, byte deltaFlag, ushort min, ushort max)
        {
            int value = 0;

            for (var i = 0; i < glyph.Points.Length; i++)
            {
                var flag = flags[i];
                if ((flag & byteFlag) > 0)
                {
                    if ((flag & deltaFlag) > 0)
                    {
                        value += File.getUint8();
                    }
                    else
                    {
                        value -= File.getUint8();
                    }
                }
                else if ((~flag & deltaFlag) > 0)
                {
                    value += File.getInt16();
                }
                else
                {
                    // value is unchanged.
                }

                if (name == "X")
                {
                    glyph.Points[i].X = value;
                }
                else
                {
                    glyph.Points[i].Y = value;
                }
            }
        }

        private uint getGlyphOffset(uint index)
        {
            Debug.Assert(Tables.ContainsKey("loca"));
            Table table = Tables["loca"];
            uint offset, old;

            if (IndexToLocFormat == 1)
            {
                old = File.Seek(table.Offset + index * 4);
                offset = File.getUint32();
            }
            else
            {
                old = File.Seek(table.Offset + index * 2);
                offset = (uint)File.getUint16() * 2;
            }

            File.Seek(old);

            return offset + Tables["glyf"].Offset;
        }

        public void DrawGlyph(ref Bitmap bm, uint index)
        {
            Glyph glyph = ReadGlyph(index);

            if(glyph == null || glyph.Type != "simple")
            {
                return;
            }

            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bm);
            System.Windows.Forms.PictureBox PicCanvas = new System.Windows.Forms.PictureBox();

        }
    }


}
