using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Server_http
{
    public class Input
    {
        public int K { get; set; } = 10;
        public decimal[] Sums { get; set; } = new decimal[] { 1, 01, 2, 02 };
        public int[] Muls { get; set; } = new int[] { 1, 4 };

        public Input()
        {

        }
    }

    [Serializable]
    public class Output
    {
        public decimal SumResult { get; set; }
        public int MulResult { get; set; }
        public decimal[] SortedInputs { get; set; }

        public Output()
        {

        }

    }

    public class ClientObject
    {
        public TcpClient client;
        public ClientObject(TcpClient tcpClient)
        {
            client = tcpClient;
        }

        public static decimal Sum(Input input)
        {
            decimal result = 0;
            foreach (var elem in input.Sums)
            {
                result += elem;
            }
            return result * input.K;
        }

        public static int Multiplication(Input input)
        {
            int result = 1;
            foreach (var elem in input.Muls)
            {
                result *= elem;
            }
            return result;
        }

        public static decimal[] Sorted(Input input)
        {
            var arr = input.Sums.Concat(input.Muls.Select(x => Convert.ToDecimal(x)).ToArray()).ToArray();
            decimal buff = 0;
            for (int i = 0; i < arr.Length - 1; i++)
            {
                for (int j = i; j < arr.Length; j++)
                {
                    if (arr[i] > arr[j])
                    {
                        buff = arr[i];
                        arr[i] = arr[j];
                        arr[j] = buff;
                    }
                }
            }
            return arr;
        }

        public static string Ping(NetworkStream stream, byte[] data)
        {
            string message = "HttpStatusCode.Ok (200)";
            data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            Console.WriteLine("Ответ клиенту: " + message);
            return message;
        }

        public static void PostInputData(NetworkStream stream, byte[] data)
        {
            Input input = new Input();
            string message = JsonSerializer.Serialize<Input>(input);
            data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            Console.WriteLine("Ответ клиенту: " + message);
        }

        public static void GetAnswer(NetworkStream stream, byte[] data, Input input, string message)
        {
            Output output = JsonSerializer.Deserialize<Output>(message);
            Console.WriteLine(Sum(input));
            Console.WriteLine(output.SumResult);
            Console.WriteLine(Multiplication(input));
            Console.WriteLine(output.MulResult);
            Console.WriteLine(Sorted(input));
            Console.WriteLine(output.SortedInputs);
            if (Sum(input) == output.SumResult && Multiplication(input) == output.MulResult && Sorted(input).SequenceEqual<Decimal>(output.SortedInputs))
            {
                string mes = "Верное решение";
                data = Encoding.UTF8.GetBytes(mes);
                stream.Write(data, 0, data.Length);
                Console.WriteLine("Ответ клиенту: " + mes);
                
            }
            else
            {
                string mes = "Не верно!";
                data = Encoding.UTF8.GetBytes(mes);
                stream.Write(data, 0, data.Length);
                Console.WriteLine("Ответ клиенту: " + mes);
            }
        }

        public static void Stop(NetworkStream stream)
        {
            stream.Close();
        }



        public void Process()
        {
            NetworkStream stream = null;
            try
            {
                Input input = new Input();
                stream = client.GetStream();
                byte[] data = new byte[64]; // буфер для получаемых данных
                while (true)
                {
                    // получаем сообщение
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    if(message == "1")
                    {
                        Ping(stream, data);
                    }
                    else if(message == "2")
                    {
                        PostInputData(stream, data);
                    }
                    else if ((message.StartsWith("{") && message.EndsWith("}"))|| (message.StartsWith("[") && message.EndsWith("]")))
                    {
                        GetAnswer(stream, data, input, message);
                        Stop(stream);
                    }
                    else
                    {
                        Console.WriteLine("Неизвестный запрос, повторите попытку");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
            }
        }
    }
}
