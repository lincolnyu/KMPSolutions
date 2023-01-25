// See https://aka.ms/new-console-template for more information

using KmpCrmCore;
using System.Text.RegularExpressions;

using DateOnly = System.DateTime;

const string CancelSymbol = "^c";

static string GetStringInput(string prompt, bool allowEmpty,  string? expectedPattern = null)
{
    var regex = expectedPattern != null? new Regex(expectedPattern) : null;
    while (true)
    {
        Console.Write(prompt);
        var s = Console.ReadLine().Trim();
        if (regex != null)
        {
            var match = regex.Match(s);
            if (!match.Success)
            {
                Console.WriteLine($"Error: Input '{s}' not matching expected pattern '{expectedPattern}'");
                continue;
            }
        }
        if (s.ToLower() == CancelSymbol)
        {
            throw new EarlyQuitException("Entry of record cancelled by user.");
        }
        if (allowEmpty && s == "")
        {
            return s;
        }
        if (s != "")
        {
            return s;
        }
    }
}

static DateOnly? GetDateInput(string prompt, bool allowEmpty)
{
    while (true)
    {
        var dateStr = GetStringInput(prompt, allowEmpty);
        if (dateStr == "" && allowEmpty)
        {
            return null;
        }
        if (DateOnly.TryParse(dateStr, out var date))
        {
            return date;
        }
        Console.WriteLine("Error: Invalid date.");
    }
}

static bool GetYesOrNo(string message)
{
    while (true)
    {
        Console.Write(message);
        var key = Console.ReadKey(false);
        Console.WriteLine();
        switch (char.ToLower(key.KeyChar))
        {
            case 'y':
                return true;
            case 'n':
                return false;
            case 'c':
                throw new EarlyQuitException("Entry of record cancelled by user.");
        }
    }
}

static CommentedValue<VisitBatch> UpdateVisitBatch(Customer customer, bool isExistingCustomer)
{
    bool addToLast = false;
    if (isExistingCustomer)
    {
        addToLast = GetYesOrNo("Add to the last batch? (Y/N)>");
    }
    CommentedValue<VisitBatch> vb;
    if (addToLast)
    {
        vb = customer.VisitBatches[customer.VisitBatches.Count - 1];
    }
    else
    {
        var comments = GetStringInput("Visit Batch Comments>", true);
        vb = new CommentedValue<VisitBatch>(new VisitBatch{ }, comments);
        while (true)
        {
            var expectedVisitsStr = GetStringInput("Expected Visits>", true);
            if (int.TryParse(expectedVisitsStr, out var expectedVisits))
            {
                vb.Value.ExpectedVisits = expectedVisits;
                break;
            }
            Console.WriteLine("Error: Expected Visits has to be a number");
        }
        customer.VisitBatches.Add(vb);
    }

    while (true)
    {
        var visitDate = GetYesOrNo("Is visit date? (Y for visit date or N for claim date)>");
        var date = GetDateInput(visitDate ? "Visit Date>" : "Claim Date>", false);

        var comments = GetStringInput("Comments>", true);
        var commentedDate = new CommentedValue<DateOnly>(date.Value, comments);

        if (visitDate)
        {
            vb.Value.VisitsMade.Add(commentedDate);
        }
        else
        {
            vb.Value.ClaimsMade.Add(commentedDate);
        }
        if (!GetYesOrNo("Add more dates? (Y/N)"))
        {
            break;
        }
    }
    return vb;
}

if (args.Length == 3 && args[0] == "upgrade")
{
    var src = args[1];
    var dst = args[2];

    var v1 = new CsvSerializerV1();
    using var sr = new StreamReader(src);
    using var sw = new StreamWriter(dst);

    var crm = v1.Deserialize(sr);
    var vc = new CsvSerializer();
    vc.Serialize(crm, sw);
}
else if (args.Length == 1)
{
    CrmRepository crm;
    {
        using var sr = new StreamReader(args[0]);
        var vc = new CsvSerializer();
        crm = vc.Deserialize(sr);
    }
    var dirty = false;
    while (true)
    {
        Console.Write(">");
        var cmd = Console.ReadLine().ToLower();
        if (cmd == "add")
        {
            try
            {
                var medicare = GetStringInput(" Medicare Number>", false);
                var isExistingCustomer = crm.Customers.TryGetValue(medicare, out var customer);
                if (isExistingCustomer)
                {
                    Console.WriteLine($"Customer already exists, named '{customer.Surname}, {customer.FirstName}'.");
                }
                else
                {
                    Console.WriteLine("Creating new customer...");
                    var surname = GetStringInput(" Surname>", false);
                    var firstName = GetStringInput(" First Name>", false);
                    var gender = GetStringInput(" Gender>", false);
                    var dob = GetDateInput(" DateOfBirth>", true);
                    var phone = GetStringInput(" Phone Number>", true);
                    var address = GetStringInput(" Address>", true);
                    var initialLetter = GetStringInput(" Initial Letter>", true);
                    var referringGPNumber = GetStringInput(" Referring GP Provider Number>", true);
                    if (!crm.Gps.TryGetValue(referringGPNumber, out var gp))
                    {
                        gp = new Gp();
                        Console.WriteLine("GP has not been found.");
                        var referringGP = GetStringInput(" Referring GP Name>", true);
                        gp.Name = referringGP;
                        crm.Gps.Add(referringGPNumber, gp);
                    }
                    customer = new Customer
                    {
                        MedicareNumber = medicare,
                        Surname = surname,
                        FirstName = firstName,
                        Gender = gender,
                        DateOfBirth = dob,
                        PhoneNumber = phone,
                        Address = address,
                        InitialLetter = initialLetter.ParseYes(),
                        ReferingGP = gp
                    };
                }
                if (GetYesOrNo("Update visit batch? (Y/N)>"))
                {
                    UpdateVisitBatch(customer, isExistingCustomer);
                }
                if (!isExistingCustomer)
                {
                    crm.Customers.Add(medicare, customer);
                }
                dirty = true;
            }
            catch (EarlyQuitException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        else if (cmd == "list")
        {
            foreach (var kvp in crm.Customers)
            {
                var customer = kvp.Value;
                Console.WriteLine($" {customer.MedicareNumber,13} | {customer.Surname},{customer.FirstName}"); 
            }
        }
        else if (cmd == "quit" || cmd == "exit" || cmd == "q")
        {
            break;
        }
    }
    dirty= true;
    if (dirty && GetYesOrNo("Save changes? (Y/N)>"))
    {
        using var sw = new StreamWriter(args[0]);
        var vc = new CsvSerializer();
        vc.Serialize(crm, sw);
    }
}

class EarlyQuitException : Exception
{
    public EarlyQuitException(string message) : base(message)
    {
    }
}