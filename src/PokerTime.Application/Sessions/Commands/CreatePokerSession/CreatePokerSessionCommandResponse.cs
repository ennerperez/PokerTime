namespace PokerTime.Application.Sessions.Commands.CreatePokerSession
{
    using Domain.ValueObjects;
    using Net.Codecrete.QrCodeGenerator;

    public sealed class CreatePokerSessionCommandResponse
    {
        public SessionIdentifier Identifier { get; }
        public QrCode QrCode { get; }
        public string Location { get; }

        public CreatePokerSessionCommandResponse(SessionIdentifier identifier, QrCode qrCode, string location)
        {
            Identifier = identifier;
            QrCode = qrCode;
            Location = location;
        }
    }

//     [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
//         "CA1815:Override equals and operator equals on value types",
//         Justification = "<Pending>")]
//     public readonly struct QrCode
//     {
//         private readonly QRCodeData _qrCodeData;
//
//         public QrCode(QRCodeData qrCodeData)
//         {
//             this._qrCodeData = qrCodeData;
//         }
//
//         public string ToBase64()
//         {
//             //TODO: Use a better implementation
// #if NET6_0_WINDOWS
//             using var base64QrCode = new Base64QRCode(this._qrCodeData);
//
//             return "data:image/png;base64," + base64QrCode.GetGraphic(5, Color.Black, Color.Transparent);
// #else
//             //return "data:image/png;base64,";
//             Net.Codecrete.QrCodeGenerator.QrCode.EncodeBinary()
//             var qr = QrCode.EncodeText("Hello, world!", QrCode.Ecc.Medium);
//             string svg = qr.ToSvgString(4);
//             File.WriteAllText("hello-world-qr.svg", svg, Encoding.UTF8);
//
//             var imgType = Base64QRCode.ImageType.Jpeg;
//             Base64QRCode qrCode = new Base64QRCode(qrCodeData);
//
//             using (var qrGenerator = new QRCodeGenerator())
//             using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(this._qrCodeData.ToString(), QRCodeGenerator.ECCLevel.Q))
//             using (QRCode qrCode = new QRCode(qrCodeData))
//             {
//                 using var base64QrCode = new Base64QRCode();
//             return "data:image/png;base64," + base64QrCode.GetGraphic(5, Color.Black, Color.Transparent);
// #endif
//         }
//     }

 }
