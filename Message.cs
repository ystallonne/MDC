using System.Collections.Generic;
using System.IO;

namespace RedCell.Devices.LedDisplay.Daktronics
{
    /// <summary>
    /// A display message.
    /// </summary>
    public class Message : List<Frame>
    {
        #region Fields
        private static int _id = 0;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <remarks>
        /// The empty constructor creates a message with a name in the form MSG_##, where ## is an incrementing integer.
        /// </remarks>
        public Message()
        {
            Name = "MSG" + _id++;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Message(string name)
        {
            Name = name;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }
        #endregion
        
        #region Methods
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("Message {0} ({1} {2})", Name, Count, Count == 1 ? "frame" : "frames");
        }

        /// <summary>
        /// Serializes this instance.
        /// </summary>
        /// <returns>System.String.</returns>
        public byte[] Serialize()
        {
            using(var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                foreach (Frame f in this)
                    bw.Write(f.Serialize());
                return ms.ToArray();
            }
        }
        #endregion
    }
}
