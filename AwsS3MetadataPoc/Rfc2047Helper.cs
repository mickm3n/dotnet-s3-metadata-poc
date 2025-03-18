using System.Text;
using MimeKit.Utils;

public static class Rfc2047Helper
{
    public static string Rfc2047Encode(string input)
    {
        if (input.All(c => c < 128))
            return input;
        byte[] encodedBytes = Rfc2047.EncodeText(Encoding.UTF8, input);
        return Encoding.ASCII.GetString(encodedBytes);
    }

    public static string Rfc2047Decode(string encoded)
    {
        if (!(encoded.StartsWith("=?") && encoded.EndsWith("?=")))
            return encoded;
        Console.WriteLine("RFC 2047 string before decoding: {0}", encoded);
        byte[] encodedBytes = Encoding.ASCII.GetBytes(encoded);
        return Rfc2047.DecodePhrase(encodedBytes);
    }
}
