namespace TestTools.Services
{
    public class Base64Service
    {
        public (string dataUrl, string contentType) ToBase64(byte[] bytes, string contentType)
        {
            var b64 = Convert.ToBase64String(bytes);
            var dataUrl = $"data:{contentType};base64,{b64}";
            return (dataUrl, contentType);
        }

        public (byte[] bytes, string? contentType) FromBase64(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                throw new ArgumentException("Base64 input is empty");

            string? contentType = null;
            string payload = base64.Trim();

            if (payload.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var commaIdx = payload.IndexOf(',');
                if (commaIdx > 0)
                {
                    var header = payload.Substring(5, commaIdx - 5); // after 'data:'
                    payload = payload.Substring(commaIdx + 1);
                    var semi = header.IndexOf(';');
                    contentType = semi > 0 ? header.Substring(0, semi) : header;
                }
            }

            // Remove possible base64 markers
            payload = payload.Replace("\r", string.Empty).Replace("\n", string.Empty);
            var bytes = Convert.FromBase64String(payload);
            return (bytes, contentType);
        }
    }
}
