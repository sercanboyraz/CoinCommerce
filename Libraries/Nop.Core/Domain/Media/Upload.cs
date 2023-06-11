using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Media
{
    public class Upload : BaseEntity
    {
        /// <summary>
        /// Gets or sets a GUID
        /// </summary>
        public Guid UploadGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether UploadUrl property should be used
        /// </summary>
        public bool UseUploadUrl { get; set; }

        /// <summary>
        /// Gets or sets a Upload URL
        /// </summary>
        public string UploadUrl { get; set; }

        /// <summary>
        /// Gets or sets the Upload binary
        /// </summary>
        public byte[] UploadBinary { get; set; }

        /// <summary>
        /// The mime-type of the Upload
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The filename of the Upload
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the extension
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Upload is new
        /// </summary>
        public bool IsNew { get; set; }
    }
}
