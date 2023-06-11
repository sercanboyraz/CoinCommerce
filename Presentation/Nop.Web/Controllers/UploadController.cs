using Microsoft.AspNetCore.Mvc;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Web.Models.Media;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nop.Web.Controllers
{
    public partial class UploadController : BasePublicController
    {
        private readonly IUpload _uploadService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly INopFileProvider _fileProvider;

        public UploadController(
            IUpload uploadService,
            IOrderService orderService,
            IProductService productService,
            INopFileProvider fileProvider)
        {
            _uploadService = uploadService;
            _orderService = orderService;
            _productService = productService;
            _fileProvider = fileProvider;
        }

        [HttpPost]
        public virtual async Task<IActionResult> UploadReceipt([FromBody] UploadDto uploadDto)
        {
            var orderItem = await _orderService.GetOrderItemByGuidAsync(Guid.Parse(uploadDto.OrderItemId));
            if (orderItem == null)
                return InvokeHttp404();

            var base64EncodedBytes = System.Convert.FromBase64String(uploadDto.Base64File);

            var fileName = $"order_{uploadDto.OrderItemId}.pdf";
            var filePath = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/receipt"), fileName);
            await using (var fileStream = new FileStream(filePath, FileMode.Create))
                await fileStream.WriteAsync(base64EncodedBytes, 0, base64EncodedBytes.Length);

            var receipt = await _uploadService.UploadReceipt(uploadDto.Base64File, filePath, fileName);
            orderItem.UploadId = receipt.Id;
            await _orderService.UpdateOrderItemAsync(orderItem);
            return View();
        }

        [HttpPost]
        public virtual async Task<IActionResult> GetUploadReceipt(string OrderItemId)
        {
            var orderItem = await _orderService.GetOrderItemByGuidAsync(Guid.Parse(OrderItemId));
            if (orderItem == null)
                return InvokeHttp404();
            if (orderItem.UploadId <= 0)
                return Content("Upload data is not available any more.");
            var getUpload = await _uploadService.GetUploadReceipt(orderItem.UploadId.Value);
            return Json(getUpload.UploadUrl);
        }
    }
}
