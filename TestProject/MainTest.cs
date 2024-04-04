using IbanConverter;

namespace TestProject
{
    [TestClass]
    public class UnitTest1
    {
        private List<string> _inputStrings;
        private List<string> _outputStrings;

        private string inputFilesDirectory = "inputFiles";

        [TestInitialize]
        public void InitializeTests()
        {
            _inputStrings = new List<string>();
            _outputStrings = new List<string>();

            // the input strings list
            _inputStrings.Add("246000/5500");
            _inputStrings.Add("CZ0655000000000000246000");
            _inputStrings.Add("LT0655000000000045400246000");
            _inputStrings.Add("JustATestString");

            // the output strings list to assert the result
            _outputStrings.Add("246000/5500;000000-0000246000/5500;CZ0655000000000000246000");
            _outputStrings.Add("LT0655000000000045400246000;NEROZPOZNÁNO;NEROZPOZNÁNO");
            _outputStrings.Add("JustATestString;NEROZPOZNÁNO;NEROZPOZNÁNO");

        }

        [TestMethod]
        public void TestInputAndOutput()
        {
            List<string> result = new List<string>();
            foreach (string inputString in _inputStrings)
            {
                BankAccount bankAccount = new BankAccount(inputString);
                string resultString = $"{bankAccount.StandardFormatAccountNumber};{bankAccount.ExtendedFormatAccountNumber};{bankAccount.IbanFormatAccountNumber}";
                if (bankAccount != null && bankAccount.IbanFormatAccountNumber != "" && bankAccount.StandardFormatAccountNumber != "" && bankAccount.ExtendedFormatAccountNumber != "")
                {
                    if (!result.Contains(resultString))
                        result.Add(resultString);
                }
                else
                    result.Add($"{inputString};NEROZPOZNÁNO;NEROZPOZNÁNO");
            }

            Assert.AreEqual(result.Count, _outputStrings.Count);

            foreach (string outputString in _outputStrings)
            {
                Assert.IsTrue(_outputStrings.Contains(outputString));
            }
        }
    }
}