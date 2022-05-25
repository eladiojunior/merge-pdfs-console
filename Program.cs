using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace merge_pdfs_console
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("+---------------------------------------------------------------+");
            Console.WriteLine("| Programa console para unir arquivos PDF em um só arquivo.     |");
            Console.WriteLine("| Como utilizar:                                                |");
            Console.WriteLine("| />merge_pdfs -a 'arquivo1.pdf' 'arquivo2.pdf' -o 'result.pdf' |");
            Console.WriteLine("| />merge_pdfs -d 'pasta' -o 'result.pdf'                       |");
            Console.WriteLine("+---------------------------------------------------------------+");
            if (args == null || !args.Any())
            {
                Console.Error.WriteLine("Nenhuma parametro enviado para o programa.");
                return;
            }
            var processo = VerificarParametros(args);
            if (processo != null)
            {
                Console.WriteLine("Iniciar o processo de merge dos [{0}] arquivos em um [{1}].", processo.ListaArquivosPdf.Count, processo.NomeArquivoSaida);
                if (UnirPdfs(processo))
                {
                    Console.WriteLine("Processamento realizado com sucesso.");
                    Console.WriteLine("Arquivos de saída: {0}", processo.NomeArquivoSaida);
                }
            }
            Console.WriteLine("+---------------------------------------------------------------+");
        }

        /// <summary>
        /// Realizar o processamento da união dos arquivos PDFs em um só.
        /// </summary>
        /// <param name="processo">Informações do processamento.</param>
        /// <returns></returns>
        private static bool UnirPdfs(ProcessoMerge processo)
        {
            PdfWriter writer = null;
            Document document = null;
            PdfReader reader;
            PdfContentByte cb = null;
            int index = 0;
            try
            {
                // Percorre a lista de arquivos a serem concatenados.
                foreach (FileInfo file in processo.ListaArquivosPdf)
                {
                    // Cria o PdfReader para ler o arquivo
                    reader = new PdfReader(file.FullName);
                    // Obtém o número de páginas deste pdf
                    var numPages = reader.NumberOfPages;
 
                    if (index == 0)
                    {
                        // Cria o objeto do novo documento
                        document = new Document(reader.GetPageSizeWithRotation(1));
                        // Cria um writer para gravar o novo arquivo
                        writer = PdfWriter.GetInstance(document, new FileStream(processo.NomeArquivoSaida, FileMode.Create));
                        // Abre o documento
                        document.Open();
                        cb = writer.DirectContent;
                    }
 
                    // Adiciona cada página do pdf origem ao pdf destino.
                    int i = 0;
                    while (i < numPages)
                    {
                        i++;
                        document.SetPageSize(reader.GetPageSizeWithRotation(i));
                        document.NewPage();
                        PdfImportedPage page = writer.GetImportedPage(reader, i);
                        int rotation = reader.GetPageRotation(i);
                        if (rotation == 90 || rotation == 270)
                            cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                        else
                            cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                    }
                    index++;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ERRO: " + ex.Message);
                return false;
            }
            finally
            {
                document?.Dispose();
            }

            return true;
            
        }

        private static ProcessoMerge VerificarParametros(string[] args)
        {
           
            //Recuperar os arquivos de entrada PDFs.
            var parmOrigemArquivo = args.FirstOrDefault(s => s.ToLower().Equals("-a")) ?? args.FirstOrDefault(s => s.ToLower().Equals("-d"));
            if (parmOrigemArquivo == null)
            {
                Console.Error.WriteLine("Informe os paramentros de '-a' para processar arquivos ou '-d' para processar pastas de PDF.");
                return null;
            }

            var listaArquivosPdf = RecuperarListaArquivosPdf(args);
            if (listaArquivosPdf == null || listaArquivosPdf.Count == 0)
            {
                Console.Error.WriteLine("Nenhum arquivo PDF encontrado para realizar união.");
                return null;
            }

            if (listaArquivosPdf.Count == 1)
            {
                Console.Error.WriteLine("Vish, esse programa serve para unir 'VARIOS' arquivos PDF em um único... lembra? Você enviou apenas um arquivo de entrada ;)");
                return null;
            }
            
            //Recuperar informações do nome de saíde do PDF.
            var parmNomeSaida = args.FirstOrDefault(s => s.ToLower().Equals("-o"));
            var nomeSaida = $"{DateTime.Now:ddMMyyyyHHmmss}.pdf";
            if (parmNomeSaida != null && parmNomeSaida.ToLower().Equals("-o"))
            {
                var strNomeSaida = RecuperarNomeSaida(args);
                if (!string.IsNullOrEmpty(strNomeSaida))
                    nomeSaida = strNomeSaida;
            }

            var result = new ProcessoMerge
            {
                NomeArquivoSaida = nomeSaida,
                ListaArquivosPdf = new List<FileInfo>(listaArquivosPdf)
            };
            return result;
            
        }

        /// <summary>
        /// Recuperar o nome da arquivos de saída.
        /// </summary>
        /// <param name="args">Argumentos enviados como parâmetro para o programa.</param>
        /// <returns></returns>
        private static string RecuperarNomeSaida(string[] args)
        {
            string nomeSaida = null;
            var hasNomeSaida = false;
            foreach (var arg in args)
            {
                if (arg.Equals("-o"))
                {
                    hasNomeSaida = true;
                    continue;
                }
                if (!hasNomeSaida) continue; //Verificar nome de saida.
                nomeSaida = arg;
                if (!nomeSaida.ToLower().EndsWith(".pdf"))
                    nomeSaida = nomeSaida + ".pdf";
                break;
            }
            return nomeSaida;
        }

        /// <summary>
        /// Recupera a lista de arquivos PDFs válidos enviado nos parâmetros do programa.
        /// </summary>
        /// <param name="args">Parametros enviados para o programa.</param>
        /// <returns></returns>
        private static List<FileInfo> RecuperarListaArquivosPdf(string[] args)
        {
            var result = new List<FileInfo>();
            var hasArquivos = false;
            var hasPasta = false;
            foreach (var arg in args)
            {
                if (arg.ToLower().Equals("-a"))
                {
                    hasArquivos = true;
                    continue;
                }
                if (arg.ToLower().Equals("-d"))
                {
                    hasArquivos = false;
                    hasPasta = true;
                    continue;
                }
                if (arg.ToLower().Equals("-o"))
                {
                    hasArquivos = false;
                    hasPasta = false;
                    continue;
                }
                
                if (hasArquivos)
                {//Verificar os arquivos.
                    var arquivoPdf = VerificarArquivoPdf(arg);
                    if (arquivoPdf != null)
                        result.Add(arquivoPdf);
                    else
                        Console.Error.WriteLine("Arquivo [{0}] não existe ou não é um PDF.", arg);
                    continue;
                }
                
                if (hasPasta)
                {//Verificar a pasta dos arquivos.
                    var pastaPdf = VerificarPastaPdfs(arg);
                    if (pastaPdf != null)
                        result.AddRange(pastaPdf);
                    else
                        Console.Error.WriteLine("Pasta [{0}] não existe ou não contém arquivos PDF.", arg);
                }
                
            }
            
            return result;
            
        }

        /// <summary>
        /// Verifica a pasta enviada nos parametros do programa com os arquivos PDFs.
        /// </summary>
        /// <param name="pathPastaPdf">Path da pasta com os arquivos PDFs;</param>
        /// <returns></returns>
        private static List<FileInfo> VerificarPastaPdfs(string pathPastaPdf)
        {
            var pastaPdf = new DirectoryInfo(pathPastaPdf);
            if (string.IsNullOrEmpty(pathPastaPdf))
                return null;
            if (!pastaPdf.Exists)
                return null;

            var result = new List<FileInfo>();
            foreach (var fileInfo in pastaPdf.GetFiles("*.pdf"))
            {
                if (fileInfo.Exists && fileInfo.Extension.Equals(".pdf"))
                    result.Add(fileInfo); 
            }
            return result;
        }

        /// <summary>
        /// Verificar se o path do arquivo existe e é um PDF.
        /// </summary>
        /// <param name="pathArquivoPdf">Path do arquivo PDF.</param>
        /// <returns></returns>
        private static FileInfo VerificarArquivoPdf(string pathArquivoPdf)
        {
            if (string.IsNullOrEmpty(pathArquivoPdf))
                return null;
            var arquivoPdf = new FileInfo(pathArquivoPdf);
            if (!arquivoPdf.Exists)
                return null;
            return !arquivoPdf.Extension.Equals(".pdf") ? null : arquivoPdf;
        }
    }
}