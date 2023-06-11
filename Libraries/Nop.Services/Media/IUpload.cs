using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Media
{
    public interface IUpload
    {
        /// <summary>
        /// Upload single favicon
        /// </summary>
        /// <param name="favicon">Favicon</param>
        Task<Upload> UploadReceipt(string base64File, string url, string fileName);

        Task<Upload> GetUploadReceipt(int id);

        Task<string> GetUploadReceiptUrl(int id);

        Task<string> GetUploadReceiptUrlWithOrderGuid(string guid);
    }
}
