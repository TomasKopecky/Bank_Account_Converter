namespace IbanConverter
{
    internal class Program
    {

        // class members definition and declaration
        private static List<string> inputAccountsList = new List<string>();
        private static List<string> outputAccountsList = new List<string>();
        private static List<string> menuList = new List<string>();
        private static List<string> errors = new List<string>();
        private static bool programExit = false;
        private static string? filePath;

        // main function for running the app
        public static void Main()
        {
            SetMenu();
            while (programExit == false)
            {
                // printing the error messages after each app operation
                if (errors.Count > 0)
                {
                    Console.WriteLine("Při běhu programu k následujícím chybám:");
                    foreach (string error in errors)
                        Console.WriteLine(error);
                }
                errors.Clear();
                PrintMenu();
                Console.WriteLine("Zadejte volbu: ");
                ChooseMenuOption(Console.ReadLine());
            }

        }

        private static bool processInputFile()
        {
            try
            {
                filePath = Environment.CurrentDirectory;
                if (!string.IsNullOrEmpty(filePath))
                {
                    List<string> filesList = Directory.GetFiles(filePath, "*.txt", SearchOption.TopDirectoryOnly).ToList();
                    foreach (string file in filesList)
                    {
                        if (!file.Contains("output"))
                            inputAccountsList.AddRange(File.ReadAllLines(file).ToList());
                    }
                }

                if (inputAccountsList != null && inputAccountsList.Count > 0)
                    return true;

                if (inputAccountsList != null && inputAccountsList.Count == 0)
                {
                    errors.Add("Nebyl zpracován žádný vstupní TXT soubor - nezapomněli jste jej vložit do složky?");
                    return false;
                }

                errors.Add("Nepodařilo se zpracovat vstupní TXT soubory");
                return false;
            }
            catch (IOException ex)
            {
                errors.Add($"Chyba při otevirání vstupního souboru, kód chyby: {ex.Message}");
                return false;
            }
        }

        private static void processInputBankAccounts()
        {
            if (inputAccountsList != null && inputAccountsList.Count > 0)
            {
                foreach (string inputBankAccount in inputAccountsList)
                {
                    if (!string.IsNullOrEmpty(inputBankAccount))
                    {
                        BankAccount bankAccount = new BankAccount(inputBankAccount);
                        if (bankAccount != null && bankAccount.IbanFormatAccountNumber != "" && bankAccount.StandardFormatAccountNumber != "" && bankAccount.ExtendedFormatAccountNumber != "")
                        {
                            if (!outputAccountsList.Contains($"{bankAccount.StandardFormatAccountNumber};{bankAccount.ExtendedFormatAccountNumber};{bankAccount.IbanFormatAccountNumber}"))
                                outputAccountsList.Add($"{bankAccount.StandardFormatAccountNumber};{bankAccount.ExtendedFormatAccountNumber};{bankAccount.IbanFormatAccountNumber}");
                        }
                        else
                            outputAccountsList.Add($"{inputBankAccount};NEROZPOZNÁNO;NEROZPOZNÁNO");
                    }
                }
                if (outputAccountsList.Count > 0)
                {
                    try
                    {
                        if (filePath != null)
                        {
                            string fileNamePath = Path.Combine(filePath, "output.txt");
                            if (File.Exists(fileNamePath))
                                File.Delete(fileNamePath);

                            File.WriteAllLines(fileNamePath, outputAccountsList.ToArray());
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Chyba při ukládání do výsledného TXT souboru - kód chyby: {ex.Message}");
                    }

                }
            }
            else
            {
                Console.WriteLine("Nelze provést zpracování účetů - nepodařilo se zpracovat vstupní TXT soubory");
            }
        }

        private static void SetMenu()
        {
            menuList.Add("--------------BANK_ACCOUNT_CONVERTER-----------------");
            menuList.Add("0: Konec programu");
            menuList.Add("1: Spustit převod");
            menuList.Add("2: Nápověda + o programu");
        }

        private static void PrintMenu()
        {
            foreach (string menuItem in menuList)
                Console.WriteLine(menuItem);
        }

        private static void PrintHelp()
        {
            Console.WriteLine("NÁPOVĚDA:");
            Console.WriteLine("a) Program převádí čísla bankovních účtů (pouze CZ a SK) ze standardního formátu 246000/5500 co IBAN formátu CZ0655000000000000246000 a naopak.");
            Console.WriteLine("b) Do adresáře, ve kterém spouštíte tento program, vložte TXT soubory (libovolné množství a libovolné názvy), do kterých vložte čísla CZ a SK bankovních účtů v libovolném formátu (standardní nebo IBAN a na každý řádek jeden)");
            Console.WriteLine("c) Zvolte možnost '1 - spustit převod' a program všechny nalezená čísla bankovních účtů ve všech TXT souborech převede na standardní formát (246000/5500), normalizovaný formát (000000-0000246000/5500) a IBAN formát (CZ0655000000000000246000) a výsledek zapíše do souboru 'output.txt' do stejného adresáře");
            Console.WriteLine("d) V případě, že nějaký řádek ze vstupních TXT souborů není rozpoznán jako bankovní účet CZ nebo SK, do výstupního souboru se vloží tento originální text a za něj se zapíše 'NEROZPOZNÁNO'");
            Console.WriteLine("e) Výstupní soubor 'output.txt' je ve formátu TXT, nicméně strukturován je jako CSV s oddělovačem ';'. Při každém spuštění funkce '1 - spustit převod' se výstupní soubor 'output.txt' vytvoří kompletně znovu (nedoplňuje se)");
            Console.WriteLine("f) V případě nalezení duplicitních účtů ve vstupních souborech jsou zachovány pouze jedinečné hodnoty (duplicity se mažou)");   
        }

        private static void ChooseMenuOption(string? argumentNumber)
        {
            int choice = -1;
            Console.Clear();
            if (argumentNumber != null && argumentNumber != "")
            {
                try
                {
                    choice = Convert.ToInt32(argumentNumber.ToString());
                }
                catch (FormatException)
                {

                }

            }
            //Int32.TryParse(argumentNumber.ToString(), out choice);
            if (choice < 0 || choice > menuList.Count - 2)
            {
                Console.WriteLine($"Zadejte volbu pomocí čísla 0 - {menuList.Count - 2} a stiskněte ENTER");
            }

            switch (choice)
            {
                case 0:
                    programExit = true;
                    return;
                case 1:
                    try
                    {
                        if (processInputFile())
                        {
                            processInputBankAccounts();
                            Console.WriteLine("Vstupní TXT soubory zpracovány a výsledek uložen do souboru ./output.txt");
                        }

                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Chyba během zpracovávání vstupních souborů, kód chyby: {ex.Message}");
                    }
                    break;
                case 2:
                    PrintHelp();
                    break;
            }
        }
    }
}