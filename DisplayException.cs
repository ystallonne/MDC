using System;

namespace RedCell.Devices.LedDisplay.Daktronics
{
    /// <summary>
    /// Class DisplayException.
    /// </summary>
    public class DisplayException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DisplayException(string message) : base(message)
        { }
    }
}
