using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Client_http
{
    /*class Client
    {
        public string urlAdress;
        public string port;
        public string method;

        public Client()
        {

        }

        public string GetInputData()
        {
            TcpClient client = new TcpClient();
            client.Connect(urlAdress, int.Parse(port));
            NetworkStream stream = client.GetStream();

        }
    }*/
    [Serializable]
    public class Input
    {
        public int K { get; set; }
        public decimal[] Sums { get; set; }
        public int[] Muls { get; set; }
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

    class Program
    {
        const int port = 8888;
        const string address = "127.0.0.1";
        string method = null;

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


        public static string SendingReceivingMessages(NetworkStream stream, string message, byte[] data)
        {
            message = String.Format("{0}", message);
            data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);
            string resultMessage = builder.ToString();
            return resultMessage;
        }

        public static bool Ping(NetworkStream stream, byte[] data)
        {
            string messageToServer = "1";
            string message = SendingReceivingMessages(stream, messageToServer, data);
            if (message == "HttpStatusCode.Ok (200)")
            {
                Console.WriteLine("Сервер работает");
                return true;
            }
            else {
                return false;
            }
        }

        public static Input GetInputData(NetworkStream stream, byte[] data)
        {
            string messageToServer = "2";
            string message = SendingReceivingMessages(stream, messageToServer, data);
            Input input = JsonSerializer.Deserialize<Input>(message);
            Console.WriteLine($"Ответ с сервера: {message}");
            return input;
        }

        public static void WriteAnswer(NetworkStream stream,  byte[] data, Input input)
        {
                Output output = new Output();
                output.SumResult = Sum(input);
                output.MulResult = Multiplication(input);
                output.SortedInputs = Sorted(input);
                string messageToServer = JsonSerializer.Serialize<Output>(output);
                data = Encoding.UTF8.GetBytes(messageToServer);
                stream.Write(data, 0, data.Length);
                data = new byte[64];
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                string resultMessage = builder.ToString();
                Console.WriteLine(resultMessage);
            
        }


        static void Main(string[] args)
        {
            TcpClient client = null;
            bool serverStatus = false;
            try
            {
                client = new TcpClient(address, port);
                byte[] data = new byte[64];
                NetworkStream stream = client.GetStream();
                if (Ping(stream, data))
                {
                    Input input = GetInputData(stream, data);
                    // буфер для получаемых данных
                        // получаем сообщение
                    WriteAnswer(stream, data, input);
                    
                }
                
                else
                {
                    Console.WriteLine("Сервер недоступен");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
            }
        }
    }
}


