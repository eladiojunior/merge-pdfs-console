using System.Collections.Generic;
using System.IO;

namespace merge_pdfs_console
{
    public class ProcessoMerge
    {
        public List<FileInfo> ListaArquivosPdf { get; set; }
        public string NomeArquivoSaida { get; set; }
    }
}