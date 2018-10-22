#region Usings
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;
using Intech.Lib.Web;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#endregion

namespace Intech.Lib.Util.Relatorios
{
    public class GeradorRelatorio
    {
        public static FileStream Gerar(string nomeArquivoRepx, string root, List<KeyValuePair<string, object>> parametros)
        {
            var relatorio = XtraReport.FromFile($"Relatorios/{nomeArquivoRepx}.repx");

            ((SqlDataSource)relatorio.DataSource).Connection.ConnectionString = AppSettings.Get().ConnectionString;

            foreach(var parametro in parametros)
            {
                ((SqlDataSource)relatorio.DataSource).Queries[0]
                    .Parameters
                    .Single(x => x.Name == parametro.Key)
                    .Value = parametro.Value;
            }

            relatorio.FillDataSource();

            if (((SqlDataSource)relatorio.DataSource).Result[0].Count() == 0)
                throw new Exception("Nenhum registro encontrado.");

            var folderName = "Temp";
            var tempFolder = Path.Combine(root, folderName);
            var nomeArquivo = $"{nomeArquivoRepx}_{Guid.NewGuid().ToString()}.pdf";
            var arquivo = Path.Combine(tempFolder, nomeArquivo);

            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            var tfc = new TempFileCollection(tempFolder, false);
            tfc.AddFile(arquivo, false);

            relatorio.ExportToPdf(arquivo);

            return File.OpenRead(arquivo);
        }
    }
}
