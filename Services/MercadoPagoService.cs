using QRCoder;

namespace API_Rifa.Services
{
    public class MercadoPagoService
    {
        private readonly HttpClient _httpClient;
        public MercadoPagoService(HttpClient httpClient) 
        {
            _httpClient = httpClient;
        }

        public string GerarQRCode(string codigoPix)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(codigoPix, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);

            return Convert.ToBase64String(qrCodeBytes);
        }

    }
}
