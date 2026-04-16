namespace PruebasGraficas.Classes
{
    /// <summary>
    /// Represents country phone information for display
    /// </summary>
    public record CountryPhoneInfo
    {
        /// <summary>
        /// ARG, USA, AUS
        /// </summary>
        public string IsoCode { get; set; } = string.Empty;

        /// <summary>
        /// +1,+53,etc
        /// </summary>
        public string DialCode { get; set; } = string.Empty;

        /// <summary>
        /// Argentina, United States, etc
        /// </summary>
        public string CountryName { get; set; } = string.Empty;

    }

}
