using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebConsoleApp1
{
    class Program
    {
        private static HttpListener listener;

        static void Main(string[] args)
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:168/");
            listener.Start();
            listener.BeginGetContext(Callback, null);
            Console.WriteLine("已启动");
            Console.ReadKey();
        }

        private static void Callback(IAsyncResult result)
        {
            listener.BeginGetContext(Callback, null);
            var context = listener.EndGetContext(result);
            var request = context.Request;
            switch (request.HttpMethod.ToUpper())
            {
                case "GET":
                    Console.WriteLine(request.RawUrl);
                    break;
                default:
                    return;
            }
            var response = context.Response;
            response.StatusCode = 200;
            response.ContentType = "application/json;charset=utf-8";
            response.ContentEncoding = Encoding.UTF8;
            response.AppendHeader("content-type", response.ContentType);
            var streamWriter = new StreamWriter(response.OutputStream, Encoding.UTF8);
            using (streamWriter)
            {
                streamWriter.Write("{}");
            }
        }
    }

    class Axises
    {
        public string ModelId { get; set;}
        public float X { get; set; }
    }
}
