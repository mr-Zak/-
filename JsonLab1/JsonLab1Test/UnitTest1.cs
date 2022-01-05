using NUnit.Framework;
using JsonLab1;

namespace JsonLab1Test
{
    public class Tests
    {
        [TestFixture]

        public class LabTest
        {
            [TestCase("XML", "<Input>< K > 10 </ K >< Sums >< decimal > 1,01 </ decimal >< decimal > 2,02 </ decimal ></ Sums >< Muls >< int > 1 </ int >< int > 4 </ int ></ Muls ></ Input >",
                "<Output>\r\n  <SumResult>30.30</SumResult>\r\n  <MulResult>4</MulResult>\r\n  <SortedInputs>\r\n    <decimal>1</decimal>\r\n    <decimal>1.01</decimal>\r\n    <decimal>2.02</decimal>\r\n    <decimal>4</decimal>\r\n  </SortedInputs>\r\n</Output>")]
            [TestCase("JSON", "{\"K\":10,\"Sums\":[1.01,2.02],\"Muls\":[1,4]}", "{\"SumResult\":30.30,\"MulResult\":4,\"SortedInputs\":[1,1.01,2.02,4]}")]

            public void TestInitials(string format, string input, string resultstring)
            {
                string result = Operations.StartParse(format, input);
                Assert.AreEqual(resultstring, result);
            }
        }
    }
}