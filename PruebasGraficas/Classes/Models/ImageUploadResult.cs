namespace PruebasGraficas.Classes.Models
{
    /// <summary>
    /// Carries the output of <see cref="ImageUploadInput"/> after the user selects (and optionally crops) an image.
    /// </summary>
    public sealed class ImageUploadResult
    {
        /// <summary>Raw bytes of the (optionally cropped) image.</summary>
        public required byte[] Bytes { get; init; }

        /// <summary>Original file name, e.g. "avatar.png".</summary>
        public required string FileName { get; init; }

        /// <summary>MIME type, e.g. "image/png".</summary>
        public required string ContentType { get; init; }
    }

}
