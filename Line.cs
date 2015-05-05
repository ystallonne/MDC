using System;
using System.Text;
using System.Text.RegularExpressions;

namespace RedCell.Devices.LedDisplay.Daktronics
{
    public class Line
    {
        #region Constants

        public const char STX = '\x02';
        public const char ETX = '\x03';
        public const char ESC = '\x1b';
        #endregion

        #region Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="Line" /> class.
        /// </summary>
        /// <param name="lineNo">The line no.</param>
        /// <param name="text">The text.</param>
        /// <param name="font">The font.</param>
        /// <param name="justification">The justification.</param>
        public Line(int lineNo, string text, Fonts font = Fonts.SmallVariable, Justifications justification = Justifications.Left)
        {
            Font = font;
            LineNo = lineNo;
            Text = text;
            Justification = justification;

            switch (font)
            {
                case Fonts.SmallVariable:
                case Fonts.SmallVariableBold:
                case Fonts.SmallFixed:
                case Fonts.SmallGraphic:
                    Height = 8;
                    break;
                case Fonts.MediumVariable:
                case Fonts.MediumFixed:
                case Fonts.MediumGraphic:
                    Height = 16;
                    break;
                case Fonts.LargeVariable:
                    Height = 24;
                    break;
                case Fonts.ExtraLargeVariable:
                    Height = 32;
                    break;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the justification.
        /// </summary>
        /// <value>The justification.</value>
        public Justifications Justification { get; set; }

        /// <summary>
        /// Gets or sets the line no.
        /// </summary>
        /// <value>The line no.</value>
        public int LineNo { get; set; }

        /// <summary>
        /// Gets or sets the ON duration of the flash (or zero for none).
        /// </summary>
        /// <value>The flash rate.</value>
        public int FlashRateOn { get; set; }

        /// <summary>
        /// Gets or sets the OFF duration of the flash (or zero for none).
        /// </summary>
        /// <value>The flash rate.</value>
        public int FlashRateOff { get; set; }

        /// <summary>
        /// Gets or sets the colour.
        /// </summary>
        /// <value>The colour.</value>
        public Colours Colour { get; set; }

        /// <summary>
        /// Gets or sets the entry effect.
        /// </summary>
        /// <value>The entry effect.</value>
        public EntryEffects EntryEffect { get; set; }

        /// <summary>
        /// Gets or sets the override text.
        /// </summary>
        /// <remarks>
        /// Use this field to create a custom line using the native language. All other properties will be ignored.
        /// </remarks>
        /// <value>The override.</value>
        public byte[] Override { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the font.
        /// </summary>
        /// <value>The font.</value>
        public Fonts Font { get; set; }
        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Text;
        }

        /// <summary>
        /// Serializes this line.
        /// </summary>
        /// <returns>System.String.</returns>
        public byte[] Serialize()
        {
            if (Override != null)
                return Override;

            if (Regex.IsMatch(Text, "[" + ESC + STX + ETX + "]"))
                throw new InvalidOperationException("The message text cannot contain control characters.");

            var output = new StringBuilder();
            output.Append(STX);                                 // Start of text
            output.AppendFormat("{0:D2}", LineNo);              // Line number
            output.AppendFormat("{0:D2}", (int)EntryEffect);         // Entry effect
            output.Append(ESC);
            output.AppendFormat("J{0}", (char)Justification);
            output.Append(ESC);
            output.AppendFormat("N{0}", (int)Font);
            output.Append(ESC);
            output.AppendFormat("h{0:D3}", Height);
            
            if (FlashRateOn != 0 && FlashRateOff != 0)
            {
                if (FlashRateOn <= 0 || FlashRateOn >= 100)
                    throw new ArgumentOutOfRangeException("FlashRateOn must be a value between 1, and 99.", "FlashRateOn");
                if (FlashRateOff <= 0 || FlashRateOff >= 100)
                    throw new ArgumentOutOfRangeException("FlashRateOff must be a value between 1, and 99.", "FlashRateOff");

                output.Append(ESC);
                output.Append('F');
                output.Append(ESC);
                output.AppendFormat("R{0:D2}{1:D2}", FlashRateOn, FlashRateOff);
            }
            output.Append(Text);
            output.Append(ETX);
            return Encoding.UTF8.GetBytes(output.ToString());
        }

        #endregion
    }
}
