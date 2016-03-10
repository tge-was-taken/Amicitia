using System;

namespace AtlusLibSharp.PS2.Graphics.Registers
{
    using AtlusLibSharp.Utilities;
    using System.IO;

    /// <summary>
    /// This register specifies the position and scanning direction of the rectangular area in each buffer where buffer transmission is performed.
    /// </summary>
    public class TRXPOSRegister
    {
        private ulong _rawData;

        /// <summary>
        /// X Coordinate of Upper Left Point of Source Rectangular Area
        /// </summary>
        public ulong SourceRectangleX
        {
            get { return BitHelper.GetBits(_rawData, 11, 0); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 11, value, 0); }
        }

        /// <summary>
        /// Y Coordinate of Upper Left Point of Source Rectangular Area
        /// </summary>
        public ulong SourceRectangleY
        {
            get { return BitHelper.GetBits(_rawData, 11, 16); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 11, value, 16); }
        }

        /// <summary>
        /// X Coordinate of Upper Left Point of Destination Rectangular Area
        /// </summary>
        public ulong DestinationRectangleX
        {
            get { return BitHelper.GetBits(_rawData, 11, 32); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 11, value, 32); }
        }

        /// <summary>
        /// Y Coordinate of Upper Left Point of Destination Rectangular Area
        /// </summary>
        public ulong DestinationRectangleY
        {
            get { return BitHelper.GetBits(_rawData, 11, 48); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 11, value, 48); }
        }

        /// <summary>
        /// Pixel Transmission Order (Enabled only in Local -> Local Transmission.)
        /// </summary>
        public PS2TRXPOSPixelTransmissionOrder PixelTransmissionOrder
        {
            get { return (PS2TRXPOSPixelTransmissionOrder)BitHelper.GetBits(_rawData, 2, 59); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 2, (ulong)value, 59); }
        }

        public TRXPOSRegister(
            ulong srcRecX, ulong srcRecY, 
            ulong dstRecX, ulong dstRecY, 
            PS2TRXPOSPixelTransmissionOrder pixelTransOrder)
        {
            SourceRectangleX = srcRecX;
            SourceRectangleY = srcRecY;
            DestinationRectangleX = dstRecX;
            DestinationRectangleY = dstRecY;
            PixelTransmissionOrder = pixelTransOrder;
        }

        internal TRXPOSRegister(BinaryReader reader)
        {
            _rawData = reader.ReadUInt64();
        }

        internal byte[] GetBytes()
        {
            return BitConverter.GetBytes(_rawData);
        }
    }
}
