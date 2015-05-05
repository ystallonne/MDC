using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace RedCell.Devices.LedDisplay.Daktronics
{
    /// <summary>
    /// Class Display.
    /// </summary>
    public class Display : IDisposable
    {
        #region Fields
        private TcpClient _tcp;
        private byte _tag;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Display"/> class.
        /// </summary>
        public Display()
        {
            Index = 1;
            Port = 3001;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Display"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="index">The index.</param>
        public Display(string host, int port = 3001, byte index = 1)
        {
            Host = host;
            Port = port;
            Index = index;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        /// <value>The host.</value>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>The index.</value>
        public byte Index { get; set; }
        #endregion

        #region Methods

        /// <summary>
        /// Connects to the display
        /// </summary>
        /// <returns><c>true</c> if successful <c>false</c> otherwise.</returns>
        public bool Connect()
        {
            _tcp = new TcpClient();
            _tcp.Connect(Host, Port);
            return true;
        }

        /// <summary>
        /// Checksums the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.Byte.</returns>
        private byte Checksum(IEnumerable<byte> value)
        {
            byte c = 0;
            foreach (byte b in value)
                c -= b;
            return c;
        }

        /// <summary>
        /// Tests the pattern.
        /// </summary>
        public void TestPattern(byte pattern)
        {
            var data = new byte[]
            {
                (byte)CommandsV1.TestPattern,
                pattern,
                0, 0, 0, 0, 0, 0,
            };

            SendHeader(Headers.Poll, data.Length);
            SendData(data);
            ValidateResponse((byte)CommandsV1.TestPattern, 0, 0, 0, 0, 0, 0, 0);
        }

        /// <summary>
        /// Identifies this instance.
        /// </summary>
        public void Identify()
        {
            SendHeader(Headers.Identify, 0);
            ValidateResponse(0x02, 0xff);
        }

        /// <summary>
        /// Sends the header.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="length">The length.</param>
        private void SendHeader(Headers header, int length)
        {
            var s = _tcp.GetStream();
            var sw = new BinaryWriter(s);

            //Synchronization bytes
            sw.Write((byte)0x55);
            sw.Write((byte)0xaa);

            // Address byte
            sw.Write(Index);

            // Data length
            byte count = (byte)length;
            sw.Write(count);

            // Tag/command
            byte tag = (byte)(++_tag << 4);
            byte tc = (byte)(tag | (byte)header);
            sw.Write(tc);

            byte checksum = (byte)(0 - 0x55 - 0xaa - Index - count - tc);
            sw.Write(checksum);
        }

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="data">The data.</param>
        private void SendData(byte[] data)
        {
            var s = _tcp.GetStream();
            var sw = new BinaryWriter(s);
            sw.Write(data);
            sw.Write(Checksum(data));
        }

        /// <summary>
        /// Reads this instance.
        /// </summary>
        /// <returns>System.Byte[].</returns>
        public byte[] Read()
        {
            var buffer = new byte[100];
            var s = _tcp.GetStream();
            int len = s.Read(buffer, 0, buffer.Length);
            return buffer.Take(len).ToArray();
        }

        /// <summary>
        /// Blanks this instance.
        /// </summary>
        public void Blank()
        {
            var data = new byte[]
            {
                (byte)CommandsV1.BlankSign,
                0,
                0, 0, 0, 0, 0, 0
            };

            SendHeader(Headers.Poll, data.Length);
            SendData(data);
            ValidateResponse(data);
        } 
        
        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset(bool clearMemory)
        {
            var data = new byte[]
            {
                (byte)CommandsV1.ResetSign,
                0,
                clearMemory ? (byte)1 : (byte)0, 
                0, 0, 0, 0, 0
            };

            SendHeader(Headers.Poll, data.Length);
            SendData(data);
            ValidateResponse((byte)CommandsV1.ResetSign, 0, 0, 0, 0, 0, 0, 0);
        }

        /// <summary>
        /// Runs the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void RunMessage(Message message)
        {
            using (var ms = new MemoryStream())
            using(var bw = new BinaryWriter(ms))
            {
                bw.Write((byte)CommandsV1.RunMessage);
                bw.Write((byte)0);
                bw.Write(Encoding.UTF8.GetBytes(("DEFAULT\\" + message.Name + "\0\0\0\0\0\0\0\0").Substring(0, 17)));
                //bw.Write((message.Name + "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0").Substring(0, 17));
                bw.Write((byte)0); // indefinite
                bw.Write((byte)1);  // immediate
                bw.Write(new byte[11]);
                var data = ms.ToArray();
                SendHeader(Headers.Poll, data.Length);
                SendData(data);
                ValidateResponse((byte)CommandsV1.RunMessage, 0, 0, 0, 0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendMessage(Message message)
        {
            var messagedata = message.Serialize();

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((byte) CommandsV1.SendMessage);
                bw.Write((byte) 0);
                bw.Write(Encoding.UTF8.GetBytes(("DEFAULT\\" + message.Name + "\0\0\0\0\0\0\0\0").Substring(0, 17)));
                var n = DateTime.Now;
                bw.Write(new byte[]
                {(byte) n.Second, (byte) n.Minute, (byte) n.Hour, (byte) n.Day, (byte) n.Month, (byte) (n.Year - 1900)});
                bw.Write((short) 1); // block count
                bw.Write((short) 0xf7); // block size
                bw.Write((int) messagedata.Length);
                bw.Write(new byte[31]);
                var data = ms.ToArray();
                SendHeader(Headers.Poll, data.Length);
                SendData(data);
                ValidateResponse((byte)CommandsV1.SendMessage, 0, 0, 0, 0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Sends the file block.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendFileBlock(Message message)
        {
            SendFileBlock(message.Serialize());
        }

        /// <summary>
        /// Sends the file block.
        /// </summary>
        /// <param name="block">The block.</param>
        public void SendFileBlock(byte[] block)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((byte)CommandsV1.SendFileBlock);
                bw.Write((byte)0);
                bw.Write((short)1);  // block no
                bw.Write(new byte[4]);
                bw.Write(block);
                var data = ms.ToArray();
                SendHeader(Headers.Poll, data.Length);
                SendData(data);
                ValidateResponse((byte)CommandsV1.SendFileBlock, 0, 0, 0, 0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Validates the response.
        /// </summary>
        /// <param name="expected">The expected.</param>
        private void ValidateResponse(params byte[] expected)
        {
            var response = Read();

            // Check the signature.
            if (response[0] != 0x55) throw new DisplayException("Invalid magic number");
            if (response[1] != 0xaa) throw new DisplayException("Invalid magic number");

            // Check the address.
            if (response[2] != 0x01) throw new DisplayException("Invalid display index.");

            // Check the byte count.
            if (response.Length != response[3] + 7) throw new DisplayException("Invalid response length.");

            // Check the serial number.
            if ((_tag & 0x0f) != response[4] >> 4) throw new DisplayException("Invalid header message serial number.");

            // Check the status code.
            if ((response[4] & 0x0f) != 1 && (response[4] & 0x0f) != 3) throw new DisplayException("Invalid header status code.");

            // Check checksum
            if (response[5] != Checksum(response.Take(5))) throw new DisplayException("Invalid header checksum.");

            // Check body checksum.
            byte[] body = response.Skip(6).ToArray();
            byte[] content = body.Take(body.Length - 1).ToArray();
            byte check = body[body.Length - 1];
            if(check != Checksum(content))
                throw new DisplayException("Invalid body checksum.");

            // Check that expected response matches actual.
            if (body.Length - 1 != expected.Length) throw new DisplayException("Invalid body length.");
            if (expected.Where((t, i) => content[i] != t).Any())
                throw new DisplayException("Unexpected response in body.");

            // all good!
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public void Disconnect()
        {
            _tcp.Close();
        }
        #endregion

        #region Implement IDisposable
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // The docs say TcpClient implements IDisposable, but it doesn't appear to.
        }
        #endregion
    }
}
