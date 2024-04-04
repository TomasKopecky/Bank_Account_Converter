namespace IbanConverter
{
    internal class Program
    {
        private readonly string _filePath;
        private readonly List<string> _menuList = new List<string>();
        private readonly List<string> _errors = new List<string>();

        public Program()
        {
            _filePath = Environment.CurrentDirectory;
            InitializeMenu();
        }

        public static void Main()
        {
            var program = new Program();
            program.Run();
        }

        private void Run()
        {
            bool programExit = false;
            while (!programExit)
            {
                DisplayErrors();
                PrintMenu();
                programExit = ProcessMenuOption();
            }
        }

        private void DisplayErrors()
        {
            if (_errors.Any())
            {
                Console.WriteLine("Při běhu programu k následujícím chybám:");
                _errors.ForEach(Console.WriteLine);
            }
            // clearing all the errors after each processing
            _errors.Clear();
        }

        private void InitializeMenu()
        {
            _menuList.Add("------------------------------------------------BANK_ACCOUNT_CONVERTER------------------------------------------------");
            _menuList.Add("0: Konec programu");
            _menuList.Add("1: Spustit převod");
            _menuList.Add("2: Nápověda + o programu");
        }

        private void PrintMenu()
        {
            _menuList.ForEach(Console.WriteLine);
        }

        private bool ProcessMenuOption()
        {
            Console.WriteLine("Zadejte volbu: ");
            string input = Console.ReadLine() ?? string.Empty;
            Console.Clear();
            if (!int.TryParse(input, out int choice) || choice < 0 || choice > _menuList.Count - 2)
            {
                Console.WriteLine($"Zadejte volbu pomocí čísla 0 - {_menuList.Count - 2} a stiskněte ENTER");
                return false;
            }

            switch (choice)
            {
                case 0:
                    return true;
                case 1:
                    ProcessBankAccounts();
                    break;
                case 2:
                    PrintHelp();
                    break;
            }

            return false;
        }

        private void ProcessBankAccounts()
        {
            var inputAccountsList = ProcessInputFile();
            if (inputAccountsList == null || !inputAccountsList.Any()) return;

            var outputAccountsList = ProcessInputBankAccounts(inputAccountsList);
            if (outputAccountsList.Any())
            {
                SaveOutputFile(outputAccountsList);
            }
        }

        private List<string>? ProcessInputFile()
        {
            List<string> inputAccountsList = new List<string>();
            try
            {
                List<string> filesList = Directory.GetFiles(_filePath, "*.txt", SearchOption.TopDirectoryOnly)
                    .Where(file => !file.Contains("output"))
                    .ToList();

                foreach (var file in filesList)
                {
                    inputAccountsList.AddRange(File.ReadAllLines(file));
                }

                if (!inputAccountsList.Any())
                {
                    _errors.Add("Nebyl zpracován žádný vstupní TXT soubor - nezapomněli jste jej vložit do složky?");
                    return null;
                }

                return inputAccountsList;
            }
            catch (IOException ex)
            {
                _errors.Add($"Chyba při otevírání vstupního souboru, kód chyby: {ex.Message}");
                return null;
            }
        }

        private List<string> ProcessInputBankAccounts(List<string> inputAccountsList)
        {
            var outputAccountsList = new List<string>();

            foreach (var inputBankAccount in inputAccountsList.Where(account => !string.IsNullOrWhiteSpace(account)))
            {
                var bankAccount = new BankAccount(inputBankAccount);
                var accountDetails = $"{bankAccount.StandardFormatAccountNumber};{bankAccount.ExtendedFormatAccountNumber};{bankAccount.IbanFormatAccountNumber}";
                outputAccountsList.Add(string.IsNullOrWhiteSpace(accountDetails) ? $"{inputBankAccount};NEROZPOZNÁNO" : accountDetails);
            }

            return outputAccountsList;
        }

        private void SaveOutputFile(List<string> outputAccountsList)
        {
            try
            {
                string fileNamePath = Path.Combine(_filePath, "output.txt");
                if (File.Exists(fileNamePath))
                    File.Delete(fileNamePath);

                File.WriteAllLines(fileNamePath, outputAccountsList);
                Console.WriteLine("Vstupní TXT soubory zpracovány a výsledek uložen do souboru ./output.txt");
            }
            catch (Exception ex)
            {
                _errors.Add($"Chyba při ukládání do výsledného TXT souboru - kód chyby: {ex.Message}");
            }
        }

        private void PrintHelp()
        {
            Console.WriteLine("NÁPOVĚDA:");
            Console.WriteLine("a) Program převádí čísla bankovních účtů (pouze CZ a SK) ze standardního formátu 246000/5500 co IBAN formátu CZ0655000000000000246000 a naopak.");
            Console.WriteLine("b) Do adresáře, ve kterém spouštíte tento program, vložte TXT soubory (libovolné množství a libovolné názvy), do kterých vložte čísla CZ a SK bankovních účtů v libovolném formátu (standardní nebo IBAN a na každý řádek jeden)");
            Console.WriteLine("c) Zvolte možnost '1 - spustit převod' a program všechny nalezená čísla bankovních účtů ve všech TXT souborech převede na standardní formát (246000/5500), normalizovaný formát (000000-0000246000/5500) a IBAN formát (CZ0655000000000000246000) a výsledek zapíše do souboru 'output.txt' do stejného adresáře");
            Console.WriteLine("d) V případě, že nějaký řádek ze vstupních TXT souborů není rozpoznán jako bankovní účet CZ nebo SK, do výstupního souboru se vloží tento originální text a za něj se zapíše 'NEROZPOZNÁNO'");
            Console.WriteLine("e) Výstupní soubor 'output.txt' je ve formátu TXT, nicméně strukturován je jako CSV s oddělovačem ';'. Při každém spuštění funkce '1 - spustit převod' se výstupní soubor 'output.txt' vytvoří kompletně znovu (nedoplňuje se)");
            Console.WriteLine("f) V případě nalezení duplicitních účtů ve vstupních souborech jsou zachovány pouze jedinečné hodnoty (duplicity se mažou)");
        }
    }
}