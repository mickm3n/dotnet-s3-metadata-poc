using System.Text;
using MimeKit.Utils;

public static class Rfc2047Helper
{
    // 如果字符串全部是 ASCII，则直接返回，否则用 MimeKit 编码
    public static string Rfc2047Encode(string input)
    {
        if (input.All(c => c < 128))
            return input;
        byte[] encodedBytes = Rfc2047.EncodeText(Encoding.UTF8, input);
        return Encoding.ASCII.GetString(encodedBytes);
    }

    // 如果字符串看起来是 RFC 2047 编码的，则解码，否则直接返回
    public static string Rfc2047Decode(string encoded)
    {
        if (!encoded.StartsWith("=?"))
            return encoded;
        byte[] encodedBytes = Encoding.ASCII.GetBytes(encoded);
        return Rfc2047.DecodePhrase(encodedBytes);
    }
}
