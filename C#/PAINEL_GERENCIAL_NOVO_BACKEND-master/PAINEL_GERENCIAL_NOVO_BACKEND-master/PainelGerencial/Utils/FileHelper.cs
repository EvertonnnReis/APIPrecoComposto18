using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace PainelGerencial.Utils
{
    public static class FileHelper
    {
        public static List<FileInfo> Search(string arquivo, DirectoryInfo diretorio)
        {
            List<FileInfo> files = new List<FileInfo>();
            try
            {
                foreach (FileInfo fileInfo in diretorio.GetFiles().Where(x => x.Name == arquivo))
                {
                    files.Add(fileInfo);
                }
            }
            catch
            {
                //
            }

            return files;
        }

        public static string CreateTemporaryFile(string decodedText, string fileFullName, string path)
        {
            var tempPath = DirectoryHelper.Create(path);
            var tempPathComplete = tempPath + fileFullName;

            string memString = decodedText;
            byte[] buffer = Encoding.ASCII.GetBytes(memString);
            MemoryStream ms = new MemoryStream(buffer);
            FileStream file = new FileStream(tempPathComplete, FileMode.Create, FileAccess.ReadWrite);
            ms.WriteTo(file);
            file.Close();
            ms.Close();

            return fileFullName;
        }

        public static HttpResponseMessage Download(string path, string fileName, bool deleteAfterDownload = false)
        {
            var caminhoCompleto = fileName;
            if (!File.Exists(fileName))
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            var fileStream = new FileStream(fileName, FileMode.Open);
            var bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, Convert.ToInt32(fileStream.Length));
            fileStream.Close();

            if (deleteAfterDownload)
            {
                File.Delete(fileName);
            }

            var response = new HttpResponseMessage
            {
                Content = new ByteArrayContent(bytes)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            return response;
        }
    }
}
