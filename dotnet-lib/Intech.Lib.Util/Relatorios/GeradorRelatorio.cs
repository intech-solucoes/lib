#region Usings
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;
using Intech.Lib.Web;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO; 
#endregion

namespace Intech.Lib.Util.Relatorios
{
    public class GeradorRelatorio
    {
        public static FileStream Gerar(string nomeArquivoRepx, string root, List<KeyValuePair<string, object>> parametros)
        {
            var relatorio = XtraReport.FromFile($"Relatorios/{nomeArquivoRepx}.repx");

            ((SqlDataSource)relatorio.DataSource).Connection.ConnectionString = AppSettings.Get().ConnectionString;
            ((SqlDataSource)relatorio.DataSource).Queries[0].Parameters[0].Value = 1;

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
