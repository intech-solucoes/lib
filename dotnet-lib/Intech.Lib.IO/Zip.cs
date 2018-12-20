using System;
using System.Diagnostics;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Intech.Lib.IO
{
    /// <summary>
    /// Criar arquivos compactados a partir de um arquivo ou diretório
    /// </summary>
    public class Zip
    {
        public static int ObtemQuantidadeDeArquivos(string arquivo)
        {
            int quantidadeArquivos = 0;

            if (!File.Exists(arquivo))
                throw new Exception($"Arquivo não encontrado:{arquivo}");

            using (var zipInputStream = new ZipInputStream(File.OpenRead(arquivo)))
            {
                ZipEntry zipEntry;
                while ((zipEntry = zipInputStream.GetNextEntry()) != null)
                {
                    quantidadeArquivos++;
                }
            }

            return quantidadeArquivos;
        }

        /// <summary>
        /// Compacta um arquivo ou pasta em um zip
        /// </summary>
        /// <param name="caminho">Caminho do arquivo ou pasta a ser compactado</param>
        /// <param name="zipDestino">Arquivo que será gerado como resultado da comptactação</param>
        public static void Compacta(string caminho, string zipDestino)
        {
            using (Stream stream = File.Create(zipDestino))
            {
                using (ZipOutputStream zipStream = new ZipOutputStream(stream))
                {
                    zipStream.SetLevel(5);

                    if (File.Exists(caminho))
                    {
                        adicionaArquivoAoZip(string.Empty, caminho, zipStream);
                    }
                    else
                    {
                        compactaDiretorioRecursivo(caminho, caminho, zipStream);
                    }

                    zipStream.Finish();
                    zipStream.Close();
                }

                stream.Close();
            }
        }

        /// <summary>
        /// Extrai todo o conteúdo de um arquivo compactado no diretório definido
        /// </summary>
        /// <param name="arquivo">Arquivo compactado</param>
        /// <param name="diretorioDestino">Diretório destino da extração</param>
        public static void Descompacta(string arquivo, string diretorioDestino)
        {
            Descompacta(arquivo, diretorioDestino, x => { });
        }

        /// <summary>
        /// Extrai todo o conteúdo de um arquivo compactado no diretório definido
        /// </summary>
        /// <param name="arquivo">Arquivo compactado</param>
        /// <param name="diretorioDestino">Diretório destino da extração</param>
        public static void Descompacta(string arquivo, string diretorioDestino, Action<string> progresso)
        {
            if (!File.Exists(arquivo))
                throw new Exception($"Arquivo não encontrado:{arquivo}");

            using (var zipInputStream = new ZipInputStream(File.OpenRead(arquivo)))
            {
                ZipEntry zipEntry;
                while ((zipEntry = zipInputStream.GetNextEntry()) != null)
                {
                    Debug.WriteLine(zipEntry.Name);
                    progresso(zipEntry.Name);

                    //cria diretorio
                    string nomeDiretorio = Path.GetDirectoryName(zipEntry.Name);
                    if (nomeDiretorio.Length > 0)
                        Directory.CreateDirectory(Path.Combine(diretorioDestino, nomeDiretorio));

                    //cria arquivo
                    string nomeArquivo = Path.GetFileName(zipEntry.Name);
                    if (nomeArquivo != String.Empty)
                    {
                        nomeArquivo = Path.Combine(diretorioDestino, zipEntry.Name);
                        criaArquivoDescompactado(nomeArquivo, zipInputStream);
                    }
                }
            }
        }

        /// <summary>
        /// Adiciona um novo arquivo dentro de um ZIP já existente.
        /// </summary>
        /// <param name="diretorioRaiz">Diretorio raiz do Zip. Só copiar um arquivo de dentro do CTI, e o raiz for INTECH. Logo o arquivo será copiado para dentro do diretório CTI dentro do ZIP </param>
        /// <param name="novoArquivoParaAdicionar">Endereço completo do novo arquivo a ser adicionado.</param>
        /// <param name="caminhoArquivoZip">Caminho do arquivo ZIP</param>
        public static void AdicionaArquivoAoZipExistente(string diretorioRaiz, string novoArquivoParaAdicionar, string caminhoArquivoZip)
        {
            if (File.Exists(novoArquivoParaAdicionar))
            {
                ZipFile arquivoZip = new ZipFile(caminhoArquivoZip);
                arquivoZip.BeginUpdate();

                Int32 indiceDiretorioRaiz = novoArquivoParaAdicionar.IndexOf(diretorioRaiz);
                String caminhoVirtualDentroZip = novoArquivoParaAdicionar.Substring(0, (indiceDiretorioRaiz + diretorioRaiz.Length));

                arquivoZip.NameTransform = new ZipNameTransform(caminhoVirtualDentroZip);
                arquivoZip.Add(novoArquivoParaAdicionar);

                arquivoZip.CommitUpdate();
                arquivoZip.Close();
            }
        }

        public static void CompactaDiretorio(string arquivoZip, string diretorio, bool recursivo, string filtroArquivo, string filtroDiretorio)
        {
            //Exemplo de Filtro de arquivos: ".*\\.(xls|doc|xml)$"
            //   \\.pdf$
            // string filter = @"\\.txt$"; // Only files ending in ".txt"

            FastZip fz = new FastZip();
            fz.CreateEmptyDirectories = true;
            fz.CreateZip(arquivoZip, diretorio, recursivo, filtroArquivo, filtroDiretorio);

        }

        /// <summary>
        /// Cria um novo arquivo a partir da Stream sendo descompactada
        /// </summary>
        /// <param name="arquivo"></param>
        /// <param name="zipInputStream"></param>
        private static void criaArquivoDescompactado(string arquivo, ZipInputStream zipInputStream)
        {
            using (FileStream streamWriter = File.Create(arquivo))
            {
                int sourceBytes;
                var buffer = new byte[4096];
                do
                {
                    sourceBytes = zipInputStream.Read(buffer, 0, buffer.Length);
                    streamWriter.Write(buffer, 0, sourceBytes);
                } while (sourceBytes > 0);
            }
        }

        /// <summary>
        /// Adiciona todos os subdiretórios e seus arquivos ao zip.
        /// </summary>
        /// <param name="diretorioRaiz">Diretório sendo compactado</param>
        /// <param name="diretorioAtual">Próprio diretório raiz ou subdiretório do mesmo</param>
        /// <param name="zipStream">zipStream do zip sendo criado</param>
        /// <remarks>
        /// Na primeira chamada o diretório raiz deve ser igual ao diretório atual. Em seguida 
        /// o método irá entrar nos subdiretórios fazendo chamadas recursivas a si mesmo.
        /// </remarks>
        private static void compactaDiretorioRecursivo(string diretorioRaiz, string diretorioAtual,
                                                       ZipOutputStream zipStream)
        {
            var directoryInfoAtual = new DirectoryInfo(diretorioAtual);
            var directoryInfoRaiz = new DirectoryInfo(diretorioRaiz);

            DirectoryInfo[] subDiretorios = directoryInfoAtual.GetDirectories();

            foreach (DirectoryInfo diretorio in subDiretorios)
            {
                compactaDiretorioRecursivo(diretorioRaiz, diretorio.FullName, zipStream);
            }

            string caminhoRelativo = obtemCaminhoRelativo(directoryInfoRaiz, directoryInfoAtual);

            ZipEntry dirEntry;
            dirEntry = new ZipEntry(caminhoRelativo);
            dirEntry.DateTime = DateTime.Now;
            zipStream.PutNextEntry(dirEntry);

            foreach (string file in Directory.GetFiles(diretorioAtual))
            {
                adicionaArquivoAoZip(caminhoRelativo, file, zipStream);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="diretorioRaiz"></param>
        /// <param name="subDiretorio"></param>
        /// <returns></returns>
        private static string obtemCaminhoRelativo(DirectoryInfo diretorioRaiz, DirectoryInfo subDiretorio)
        {
            string tmp = subDiretorio.FullName.Substring(diretorioRaiz.FullName.Length);
            if (tmp.StartsWith(Path.DirectorySeparatorChar.ToString()))
            {
                tmp = tmp.Substring(1);
            }
            return Path.Combine(diretorioRaiz.Name, tmp) + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Adiciona um arquivo ao zip
        /// </summary>
        /// <param name="caminhoRelativo">Caminho relativo do arquivo em relação ao diretório sendo compactado.</param>
        /// <param name="arquivo">Arquivo sendo compactado</param>
        /// <param name="zipStream"></param>
        private static void adicionaArquivoAoZip(string caminhoRelativo, string arquivo, ZipOutputStream zipStream)
        {
            string caminhoRelativoArquivo = Path.Combine(caminhoRelativo, Path.GetFileName(arquivo));

            var entry = new ZipEntry(caminhoRelativoArquivo);
            entry.DateTime = DateTime.Now;
            zipStream.PutNextEntry(entry);


            using (FileStream fs = File.OpenRead(arquivo))
            {
                int sourceBytes;
                var buffer = new byte[4096];
                do
                {
                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                    zipStream.Write(buffer, 0, sourceBytes);
                } while (sourceBytes > 0);
            }
        }
    }
}
