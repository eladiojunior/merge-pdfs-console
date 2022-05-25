# Merge de Arquivos PDFs

Programa para unir arquivos PDFs diferentes em um só arquivo.

## Como utilizar o programa

1. Com o executável do programa na sua máquina (merge_pdf.exe);
2. Para unir arquivos PDFs de diferentes locais em um único arquivo (Result.pdf):
3. `/>merge_pdf.exe -a "C:\temp1\arquivo1.pdf" "C:\temp2\arquivo2.pdf" -o "C:\Result.pdf"`
4. Para unir todos arquivos PDFs de uma pasta em um único arquivo (Result.pdf):
5. `/>merge_pdf.exe -d "C:\temp\" -o "C:\Result.pdf"`

Caso não informe o parâmetro '-o' será criado um nome de saída, padrão: ddMMyyyyHHmmss.pdf;