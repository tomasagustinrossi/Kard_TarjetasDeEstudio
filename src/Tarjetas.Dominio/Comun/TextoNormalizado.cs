using System.Globalization;
using System.Text;

namespace Tarjetas.Dominio.Comun;

/// <summary>Normaliza texto para compararlo: minúsculas, sin tildes ni signos, con espacios colapsados.</summary>
public static class TextoNormalizado
{
    public static string De(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";

        var sb = new StringBuilder(texto.Length);
        foreach (var c in texto.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            sb.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : ' ');
        }
        return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
