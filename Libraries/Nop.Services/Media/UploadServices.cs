using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Plugins;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Media
{
    public partial class UploadServices : IUpload
    {
        private readonly IRepository<Upload> _uploadRepository;
        private readonly IRepository<OrderItem> _orderItem;

        public UploadServices(IRepository<Upload> uploadRepository, IRepository<OrderItem> orderItem)
        {
            _uploadRepository = uploadRepository;
            _orderItem = orderItem;
        }

        public async Task<Upload> GetUploadReceipt(int id)
        {
            return await _uploadRepository.GetByIdAsync(id);
        }

        public async Task<string> GetUploadReceiptUrl(int id)
        {
            var result = "";
            var getData = await _uploadRepository.GetByIdAsync(id);
            if (getData != null)
                result = getData.UploadUrl;
            return result;
        }

        public async Task<string> GetUploadReceiptUrlWithOrderGuid(string guid)
        {
            var result = "";
            var orderItem = _orderItem.GetAll().Where(x => x.OrderItemGuid == Guid.Parse(guid)).FirstOrDefault();
            var getData = await _uploadRepository.GetByIdAsync(orderItem.UploadId);
            if (getData != null)
                result = getData.UploadUrl;
            return result;
        }

        public async Task<Upload> UploadReceipt(string base64File, string url, string fileName)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64File);

            var upload = new Upload();
            upload.UploadGuid = Guid.NewGuid();
            upload.IsNew = true;
            upload.UploadUrl = url;
            upload.ContentType = "application/pdf";
            upload.Filename = fileName;
            upload.UploadBinary = base64EncodedBytes;

            await _uploadRepository.InsertAsync(upload);
            return upload;
        }
    }
}
