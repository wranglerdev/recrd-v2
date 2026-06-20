using Microsoft.VisualBasic.FileIO;
using Recrd.Domain.Entities;

namespace Recrd.Application.TestData;

/// <summary>
/// Importa massa a partir de CSV: 1ª linha = variáveis, 2ª linha = valores,
/// N colunas (PRD §7). Usa TextFieldParser (stdlib) p/ tratar aspas/vírgulas.
/// </summary>
public static class MassaCsv
{
    public static Massa Parse(string name, string csv)
    {
        using var parser = new TextFieldParser(new StringReader(csv))
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true,
        };
        parser.SetDelimiters(",");

        if (parser.EndOfData)
            throw new FormatException("CSV vazio.");
        var headers = parser.ReadFields()!;

        if (parser.EndOfData)
            throw new FormatException("CSV precisa de uma linha de valores (PRD §7).");
        var values = parser.ReadFields()!;

        if (headers.Length != values.Length)
            throw new FormatException($"Colunas ({headers.Length}) != valores ({values.Length}).");

        var massa = new Massa { Name = name };
        for (var i = 0; i < headers.Length; i++)
            massa.Variables[headers[i]] = values[i];
        return massa;
    }
}
