﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Device.Net;
using Usb.Net;
using Usb.Net.Windows;

namespace PushWhacker
{
    class PushDisplay
    {
        private const int AbletonVendorID = 0x2982;
        private const int Push2ProductId = 0x1967;
        private static byte[] FrameHeader = new byte[] { 0xFF, 0xCC, 0xAA, 0x88,
                                                        0x00, 0x00, 0x00, 0x00,
                                                        0x00, 0x00, 0x00, 0x00,
                                                        0x00, 0x00, 0x00, 0x00 };

        private static byte[] ShapingPattern = new byte[] { 0xE7, 0xF3, 0xE7, 0xFF };

        static string PushDeviceId;

        private static WindowsUsbInterfaceManager usbInterfaceManager;
        private static UsbDevice usbDevice;
        private static Bitmap bmp;

        public static bool Open()
        {
            return OpenAsync().Result;
        }

        public static async Task<bool> OpenAsync()
        {
            var logger = new DebugLogger();
            var tracer = new DebugTracer();
            WindowsUsbDeviceFactory.Register(logger, tracer);
            var deviceDefinitions = new FilterDeviceDefinition { DeviceType = DeviceType.Usb, VendorId = AbletonVendorID, ProductId = Push2ProductId };
            var deviceDefinition = DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(deviceDefinitions).Result;
            
            if (deviceDefinition == null || deviceDefinition.Count() < 1)
            {
                return false;
            }

            PushDeviceId = deviceDefinition.First().DeviceId;

            //This is the only platform specific part. Each platform has a UsbInterfaceManager
            usbInterfaceManager = new WindowsUsbInterfaceManager
            (
                PushDeviceId,
                logger,
                tracer,
                null,
                null
            );

            usbDevice = new UsbDevice(PushDeviceId, usbInterfaceManager, logger, tracer);

            await usbDevice.InitializeAsync();

            bmp = new Bitmap(960, 160);

            return true;
        }

        public static void Close()
        {
            usbDevice.Close();
            usbDevice.Dispose();
            usbInterfaceManager.Close();
            usbInterfaceManager.Dispose();
            usbDevice = null;
            usbInterfaceManager = null;
        }

        public static void WriteText(string text)
        {
            lock (bmp)
            {
                //Create a buffer with some data in it
                using (Graphics graphics = Graphics.FromImage(bmp))
                {
                    Color bgcolor = Color.Black;
                    Color fgcolor = Color.White;
                    Font font = new Font("Arial", 48);
                    graphics.FillRectangle(new SolidBrush(bgcolor), 0, 0, bmp.Width, bmp.Height);
                    graphics.DrawString(text, font, new SolidBrush(fgcolor), 20, 20);
                    graphics.Flush();
                    font.Dispose();
                    graphics.Dispose();
                }
            }
        }

        internal static void RefreshDisplay()
        {
            RefreshDisplayAsync().Wait();
        }

        internal static async Task RefreshDisplayAsync()
        {
            Bitmap bmp2;
            lock (bmp)
            {
                bmp2 = (Bitmap)bmp.Clone();
            }
            await usbDevice.WriteAsync(FrameHeader);

            const int screenWidth = 960;
            const int screenHeight = 160;
            const int bufferPixelCount = 1024;  //  Padded from width to the end
            const int lineBufferBytesCount = bufferPixelCount * 2;
            const int bufferByteCount = 16 * 1024;
            const int linesPerBuffer = bufferByteCount / lineBufferBytesCount;

            var pixelLine = new short[bufferPixelCount];
            var buffer = new byte[bufferByteCount];

            for (var i = 0; i < screenHeight / linesPerBuffer; i++)
            {
                for (var j = 0; j < linesPerBuffer; j++)
                {
                    var row = i * linesPerBuffer + j;
                    for (var col = 0; col < screenWidth; col++)
                    {
                        pixelLine[col] = PushPixelColour(bmp2.GetPixel(col, row));
                    }
                    for (var col = screenWidth; col < bufferPixelCount; col++)
                    {
                        pixelLine[col] = 0;
                    }
                    Buffer.BlockCopy(pixelLine, 0, buffer, j * lineBufferBytesCount, lineBufferBytesCount);
                }
                for (var j = 0; j < buffer.Length; j++)
                {
                    buffer[j] ^= ShapingPattern[j % 4];
                }
                await usbDevice.WriteAsync(buffer);
            }
        }

        private static short PushPixelColour(Color colour)
        {
            var r = (colour.R >> 3) & 0x1F;
            var g = (colour.G >> 2) & 0x3F;
            var b = (colour.B >> 3) & 0x1F;

            return (short)(b << 11 | g << 5 | r);
        }
    }
}