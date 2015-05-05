using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RedCell.Devices.LedDisplay.Daktronics
{
    /// <summary>
    /// A frame contains one or more lines.
    /// </summary>
    public class Frame : List<Line>
    {
        #region Constants
        /// <summary>
        /// The default duration in seconds.
        /// </summary>
        public const int DefaultDuration = 1;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Frame"/> class.
        /// </summary>
        public Frame()
        {
            Duration = TimeSpan.FromSeconds(DefaultDuration);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Frame"/> class.
        /// </summary>
        /// <param name="duration">The duration.</param>
        public Frame(TimeSpan duration)
        {
            Duration = duration;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the duration that the frame is shown.
        /// </summary>
        /// <value>The duration.</value>
        TimeSpan Duration { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Join("\r\n", this.Select(l => l.ToString()).ToArray());
        }

        /// <summary>
        /// Serializes this instance.
        /// </summary>
        /// <returns>System.String.</returns>
        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((byte)0x0c);
                string duration = (Duration.TotalMilliseconds/100).ToString("000");
                bw.Write(Encoding.UTF8.GetBytes(duration));
                foreach (Line l in this)
                    bw.Write(l.Serialize());
                bw.Write((byte)0x03);
                return ms.ToArray();
            }
        }
        #endregion

    }
}
