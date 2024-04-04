using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace IbanConverter
{
    public class BankAccount
    {
        private readonly string _accountCountry;

        private readonly string _originalFullAccountNumber;

        private string _accountPrefix;

        private string _accountNumber;

        private string _bankCode;
        public string StandardFormatAccountNumber { get; private set; }
        public string ExtendedFormatAccountNumber { get; private set; }
        public string IbanFormatAccountNumber { get; private set; }
        public BankAccount(string accountNumber)
        {
            _accountCountry = "CZ";
            _accountPrefix = "";
            _accountNumber = "";
            _bankCode = "";
            StandardFormatAccountNumber = "";
            ExtendedFormatAccountNumber = "";
            IbanFormatAccountNumber = "";
            _originalFullAccountNumber = accountNumber.Trim();
            createConvertedAccountNumbers();
        }

        private void createConvertedAccountNumbers()
        {
            try
            {
                if (_originalFullAccountNumber != null && _originalFullAccountNumber != "")
                    if (_originalFullAccountNumber.Contains("/"))
                    {
                        processStandardAccount();
                        parseAccountToIban();
                    }
                    else
                    {
                        processIbanAccount();
                        parseIbanToStandard();
                    }
                parseExtendedFormat();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Chyba během parsování čísla bankovního účtu " + _originalFullAccountNumber + ": " + ex.Message);
            }

        }

        private void processStandardAccount()
        {
            int dashPosition = _originalFullAccountNumber.IndexOf("-");
            int slashPosition = _originalFullAccountNumber.IndexOf("/");
            int accountLength = _originalFullAccountNumber.Length;
            if (dashPosition != -1)
            {
                _accountPrefix = _originalFullAccountNumber.Substring(0, dashPosition);
                _accountNumber = _originalFullAccountNumber.Substring(dashPosition + 1, slashPosition - (dashPosition + 1));
            }
            else
            {
                _accountPrefix = "";
                _accountNumber = _originalFullAccountNumber.Substring(0, slashPosition);
            }

            _bankCode = _originalFullAccountNumber.Substring(slashPosition + 1, accountLength - (slashPosition + 1));

            if (_accountPrefix != null && _accountPrefix != "")
                StandardFormatAccountNumber = _accountPrefix + "-" + _accountNumber + "/" + _bankCode;
            else
                StandardFormatAccountNumber = _accountNumber + "/" + _bankCode;
        }

        private void parseAccountToIban()
        {
            string accountPrefix;
            string accountNumber;
            string bban;
            string numericString;
            string numericCountryCode;
            // Validate country code
            if (_accountCountry != "CZ")// && _accountCountry != "SK")
            {
                throw new ArgumentException("Nepodporovaný národní kód IBAN účtu");
            }

            numericCountryCode = ConvertLettersToNumbers(_accountCountry);
            accountPrefix = _accountPrefix.PadLeft(6, '0');
            accountNumber = _accountNumber.PadLeft(10, '0');

            // Construct the BBAN part
            bban = _bankCode + accountPrefix + accountNumber;

            numericString = bban + numericCountryCode + "00"; // Append "00" for the checksum
            int checksum = 98 - (int)(BigInteger.Parse(numericString) % 97);
            IbanFormatAccountNumber = _accountCountry + checksum.ToString("D2") + _bankCode + accountPrefix + accountNumber;
        }

        private void processIbanAccount()
        {
            if ((_originalFullAccountNumber.Length == 24))
                IbanFormatAccountNumber = _originalFullAccountNumber;
        }

        private void parseIbanToStandard()
        {
            // Basic validation (length)
            if ((IbanFormatAccountNumber.Length != 24))// || (!ibanFormatAccountNumber.StartsWith("CZ") && !ibanFormatAccountNumber.StartsWith("SK")))
            {
                throw new ArgumentException("Nevalidní IBAN formát");
            }

            //_accountCountry = ibanFormatAccountNumber.Substring(0, 2);
            _bankCode = IbanFormatAccountNumber.Substring(4, 4);
            _accountPrefix = IbanFormatAccountNumber.Substring(8, 6).TrimStart('0');
            _accountNumber = IbanFormatAccountNumber.Substring(14, 10).TrimStart('0');

            if (_accountPrefix.Length > 0)
                StandardFormatAccountNumber = _accountPrefix + "-" + _accountNumber + "/" + _bankCode;
            else
                StandardFormatAccountNumber = _accountNumber + "/" + _bankCode;
        }

        private void parseExtendedFormat()
        {
            ExtendedFormatAccountNumber = _accountPrefix.PadLeft(6, '0') + "-" + _accountNumber.PadLeft(10, '0') + "/" + _bankCode;
        }

        private static string ConvertLettersToNumbers(string input)
        {
            string output = "";
            foreach (char c in input.ToUpper())
            {
                if (char.IsLetter(c))
                {
                    int convertedValue = c - 'A' + 10;
                    output += convertedValue.ToString();
                }
                else
                {
                    output += c;
                }
            }
            return output;
        }
    }
}
