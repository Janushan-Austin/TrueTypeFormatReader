using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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

		public BinaryReader File;
		private Dictionary<string, Table> Tables;

		public uint ScalerType, CheckSumAdjustment, MagicNumber;
		public ushort SearchRange, EntrySelector, RangeShift;

		ushort Flags, UnitsPerEm, xMin, yMin, xMax, yMax, MacStyle, LowestRecPPEM,
			   FontDirectionHint, IndexToLocFormat, glyphDataFormat;

		Decimal Version, FontRevision;
		Int64 CreatedDate, ModifiedDate;

		public TrueTypeFont(byte[] buffer)
		{			
			File = new BinaryReader(buffer);
			Tables = new Dictionary<string, Table>();
			ReadOffsetTables();
			ReadHeadTable();
		}

		private void ReadOffsetTables()
		{
			ScalerType = File.getUint32();
			ushort numTables = File.getUint16();
			SearchRange = File.getUint16();
			EntrySelector = File.getUint16();
			RangeShift = File.getUint16();

			for(ushort i=0; i<numTables; i++)
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
			while(nLongs > 0)
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
	}

	
}
