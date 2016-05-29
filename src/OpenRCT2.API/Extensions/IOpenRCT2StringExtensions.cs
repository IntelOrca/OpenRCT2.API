using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.Extensions.WebEncoders;
using OpenRCT2.Core;

namespace OpenRCT2.API.Extensions
{
    public static class IOpenRCT2StringExtensions
    {
        public static string ToHtml(this IOpenRCT2String str)
        {
            var sb = new StringBuilder();
            var htmlWriter = new StringWriter(sb);
            var htmlEncoder = HtmlEncoder.Default;

            WriteColourSpanOpen(htmlWriter, (char)144);

            char[] charArray = str.Raw.ToCharArray();
            for (int i = 0; i < charArray.Length; i++)
            {
                char c = charArray[i];
                if (OpenRCT2String.IsFormatCode(c))
                {
                    if (OpenRCT2String.IsColourCode(c))
                    {
                        WriteSpanClose(htmlWriter);
                        WriteColourSpanOpen(htmlWriter, c);
                    }
                }
                else
                {
                    htmlEncoder.Encode(htmlWriter, charArray, i, 1);
                }
            }
            WriteSpanClose(htmlWriter);
            return sb.ToString();
        }

        private static void WriteColourSpanOpen(TextWriter writer, char colourCode)
        {
            string htmlColour = GetHtmlColour(colourCode);
            writer.Write("<span style=\"color: ");
            writer.Write(htmlColour);
            writer.Write("\">");
        }

        private static void WriteSpanClose(TextWriter writer)
        {
            writer.Write("</span>");
        }

        private static string GetHtmlColour(char c)
        {
            switch (c)
            {
            default:
            case FormatCodes.White: return "#FFF";
            case FormatCodes.Green: return "#0F0";
            case FormatCodes.BabyBlue: return "#0FF";
            }
        }
    }
}
