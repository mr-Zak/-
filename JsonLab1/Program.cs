using System;
using System.Linq;
using System.Text.Json;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using System.Text;

namespace JsonLab1
{
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

    public static class Operations
    {
        public static string StartParse(string format ,string str)
        {
            if (format == "XML")
            {
                Input input = new Input();
                Output output = new Output();
                str = RemoveSpaces(str);
                input.K = GetKFromXML(str);
                input.Sums = GetSumsElementsFromXML(str);
                input.Muls = GetMulsElementsFromXML(str);
                output.SumResult = Sum(input);
                output.MulResult = Multiplication(input);
                output.SortedInputs = Sorted(input);
                return SerializeAndReadXML(output);

            }
            else if (format == "JSON")
            {
                Input input = JsonSerializer.Deserialize<Input>(str);
                Output output = new Output();
                output.SumResult = Sum(input);
                output.MulResult = Multiplication(input);
                output.SortedInputs = Sorted(input);
                return SerializeAndReadJson(output);
            }
            else return null;
        }

        public static string RemoveSpaces(string input)
        {
            input = input.Replace(" ", "");
            return input;
        }

        public static int GetKFromXML(string input)
        {
            int indexS = input.IndexOf("K") + 2;
            int indexE = input.LastIndexOf("K") - 2;
            int kef = int.Parse(input.Substring(indexS, indexE - indexS));
            return kef;
        }

        public static decimal[] GetSumsElementsFromXML(string input)
        {
            decimal[] arr = new decimal[0];
            string element = "";
            int indexStart = input.IndexOf("decimal") - 7;
            int indexEnd = input.LastIndexOf("decimal");
            for (int i = indexStart, j = 0; i < indexEnd; i++)
            {
                if (char.IsNumber(input[i])) element += input[i];
                else if (input[i] == ',' && char.IsNumber(input[i - 1]) && char.IsNumber(input[i + 1])) element += input[i];

                else
                {
                    if (element.Length != 0 && element[0] != ',')
                    {
                        Array.Resize(ref arr, arr.Length + 1);
                        arr[j] = decimal.Parse(element);
                        j++;
                    }
                    element = "";
                }
            }
            return arr;
        }

        public static int[] GetMulsElementsFromXML(string input)
        {
            int[] arr = new int[0];
            string element = "";
            int indexStart = input.IndexOf("int") - 3;
            int indexEnd = input.LastIndexOf("int");
            for (int i = indexStart, j = 0; i < indexEnd; i++)
            {
                if (char.IsNumber(input[i])) element += input[i];
                else
                {
                    if (element.Length != 0)
                    {
                        Array.Resize(ref arr, arr.Length + 1);
                        arr[j] = int.Parse(element);
                        j++;
                    }
                    element = "";
                }
            }
            return arr;
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

        public static string SerializeAndReadXML(Output output)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(Output));
            using (FileStream fs = new FileStream("input.xml", FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, output);
            }
            using (FileStream fstream = File.OpenRead($"input.xml"))
            {
                byte[] array = new byte[fstream.Length];

                fstream.Read(array, 0, array.Length);

                string textFromFile = System.Text.Encoding.Default.GetString(array);
                int index = textFromFile.IndexOf("<Output");
                textFromFile = textFromFile.Substring(index);
                int index2 = textFromFile.IndexOf('>');
                textFromFile = textFromFile.Remove(7, index2 - 7);
                //Console.WriteLine(textFromFile);
                return textFromFile;
            }
        }

        public static string SerializeAndReadJson(Output output)
        {
            string json = JsonSerializer.Serialize<Output>(output);
            return json;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string formatInput = Console.ReadLine();
            string inputString = Console.ReadLine();
            Operations.StartParse(formatInput, inputString);
        }
    }
}
