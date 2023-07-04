using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Utils
{
    public static class StringExtensions
    {
        public static bool LatinContains(this string source, string dest)
        {
            CompareInfo ci = CultureInfo.InvariantCulture.CompareInfo;
            CompareOptions co = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;
            return ci.IndexOf(source, dest, co) != -1;
        }

        public static string RemoverDiacritics(this string texto)
        {
            return Encoding.UTF8.GetString(Encoding.GetEncoding("ISO-8859-8").GetBytes(texto));
        }

        public static string RemoverCaracteres(this string texto, IEnumerable<char> listaExclusao)
        {
            StringBuilder sb = new StringBuilder(texto.Length);
            foreach (char c in texto)
            {
                if (!listaExclusao.Contains(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string RemoverQuebrasLinha(this string texto)
        {
            return texto.RemoverCaracteres(new HashSet<char>(new[] { '\t', '\n', '\r' }));
        }

        public static string SubstituirCaracteres(this string texto, IEnumerable<char> listaSubstituicao, char novoCaracter)
        {
            StringBuilder sb = new StringBuilder(texto.Length);
            foreach (char c in texto)
            {
                sb.Append(!listaSubstituicao.Contains(c) ? c : novoCaracter);
            }
            return sb.ToString();
        }

        public static string SubstituirQuebrasLinha(this string texto, char novoCaracter = ' ')
        {
            return texto.SubstituirCaracteres(new HashSet<char>(new[] { '\t', '\n', '\r' }), novoCaracter);
        }
    }
}