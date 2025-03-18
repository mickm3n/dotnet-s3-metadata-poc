using System;
using System.Text;
using System.Text.RegularExpressions;

public static class Rfc2047Helper
{
    // 使用 UTF-8 與 Base64 編碼輸入字串，產生符合 RFC 2047 格式的輸出
    public static string Rfc2047Encode(string input)
    {
        // 將輸入轉為 UTF-8 位元組
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        // 進行 Base64 編碼
        string base64 = Convert.ToBase64String(bytes);
        // 按 RFC 2047 格式產生編碼字串
        return $"=?utf-8?B?{base64}?=";
    }

    // 解析 RFC 2047 格式的字串，並回傳解碼後的原始內容（僅支持 Base64 編碼）
    public static string Rfc2047Decode(string encoded)
    {
        // RFC 2047 格式： =?charset?B?<base64 encoded text>?=
        var pattern = @"=\?(?<charset>.+?)\?(?<encoding>[Bb])\?(?<text>.+?)\?=";
        var match = Regex.Match(encoded, pattern);
        if (!match.Success)
        {
            // 如果格式不符合，直接回傳原字串
            return encoded;
        }
        string charset = match.Groups["charset"].Value;
        string text = match.Groups["text"].Value;

        // 只處理 Base64 編碼（B）
        byte[] bytes = Convert.FromBase64String(text);
        try
        {
            Encoding enc = Encoding.GetEncoding(charset);
            return enc.GetString(bytes);
        }
        catch
        {
            // 若取得 charset 失敗，預設使用 UTF-8
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
