using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueTypeFormatReader
{
	public partial class TrueTypeFont
	{

		public struct Table
		{
			public uint Checksum, Offset, Length;

			public Table(uint sum, uint offset, uint length)
			{
				Checksum = sum;
				Offset = offset;
				Length = length;
			}
		}

		
		private class FormatTable
		{
			public ushort Format, PlatformID, EncodingID;
			public uint Offset;

			
		}

		private class Format4Table : FormatTable
		{
			public ushort Length, Language, SegCountX2, SearchRamge, EntrySelector, RangeShift, ReservedPad;
			public ushort[] EndCode, StartCode, idDelta, idRangeOffset, GlyphIDArray; 


			public void CreateTable(BinaryReader File, uint cmapOffset)
			{
				uint old = File.Seek(cmapOffset + Offset);
				Format = File.getUint16();
				Length = File.getUint16();
				Language = File.getUint16();
				SegCountX2 = File.getUint16();
				SearchRamge = File.getUint16();
				EntrySelector = File.getUint16();
				RangeShift = File.getUint16();
				EndCode = new ushort[SegCountX2 / 2];
				StartCode = new ushort[SegCountX2 / 2];
				idDelta = new ushort[SegCountX2 / 2];
				idRangeOffset = new ushort[SegCountX2 / 2];

				for(ushort i =0; i < EndCode.Length; i++)
				{
					EndCode[i] = File.getUint16();
				}
				ReservedPad = File.getUint16();
				for (ushort i = 0; i < StartCode.Length; i++)
				{
					StartCode[i] = File.getUint16();
				}
				for (ushort i = 0; i < idDelta.Length; i++)
				{
					idDelta[i] = File.getUint16();
				}
				for (ushort i = 0; i < idRangeOffset.Length; i++)
				{
					idRangeOffset[i] = File.getUint16();
				}
				ushort bytesRead = (ushort)(File.Tell() - (cmapOffset + Offset));

				GlyphIDArray = new ushort[(Length - bytesRead)/2]; //size if half the difference of length - bytes read since ID array's elements are two bytes long 
				for(ushort i =0; i< GlyphIDArray.Length; i++)
				{
					GlyphIDArray[i] = File.getUint16();
				}
				bytesRead = (ushort)(File.Tell() - (cmapOffset + Offset));

			}
		}

		public struct Point
		{
			public bool OnCurve;
			public int X, Y;
			public bool IsFirst;
			public Point(bool onCurve)
			{
				OnCurve = onCurve;
				X = Y = 0;
				IsFirst = false;
			}
		}

		public class Glyph
		{
			public short NumberOfContours;
			public short xMin, yMin, xMax, yMax;

			public string Type;
			public ushort[] ContourEnds;
			public Point[] Points;

			public Glyph(short num, short xmin, short xmax, short ymin, short ymax)
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


		private void ReadFormats()
		{
			Table cmap = Tables["cmap"];
			uint old = File.Seek(cmap.Offset);
			ushort version = File.getUint16(), numTables = File.getUint16();

			FormatTable[] EncodingRecords = new FormatTable[numTables];
			for(ushort i=0; i < numTables; i++)
			{
				EncodingRecords[i] = new FormatTable();
				EncodingRecords[i].PlatformID = File.getUint16();
				EncodingRecords[i].EncodingID = File.getUint16();
				EncodingRecords[i].Offset = File.getUint32();
			}


			for (ushort i = 0; i < numTables; i++)
			{
				if(EncodingRecords[i].PlatformID == 3 && EncodingRecords[i].EncodingID == 1)
				{
					Format4 = new Format4Table();
					Format4.PlatformID = EncodingRecords[i].PlatformID;
					Format4.EncodingID = EncodingRecords[i].EncodingID;
					Format4.Offset = EncodingRecords[i].Offset;
					Format4.CreateTable(File, cmap.Offset);
				}
			}
		}

	}
}
