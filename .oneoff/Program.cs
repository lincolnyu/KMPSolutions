
// This is one off tool that expands the visits info from csv for DB input 
using var sr = new StreamReader(@".\data\visitsInput.csv");
using var sw = new StreamWriter(@".\data\visitsOutput.csv");
while (!sr.EndOfStream)
{
    var line = sr.ReadLine();
    if (line == null)
    {
        break;
    }
    var s = line.Split(',');
    var patientId = s[0];
    var eventStr = s[1];
    var eventSplit = eventStr.Split('|');
    var total = eventSplit[0];
    var visitStr= eventSplit[1];
    var claimStr = eventSplit[2];
    var visits = visitStr.Split(';', StringSplitOptions.RemoveEmptyEntries);
    var claims = claimStr.Split(';', StringSplitOptions.RemoveEmptyEntries);
    foreach (var visit in visits)
    {
        sw.WriteLine($"{patientId},{visit}");
    }
    foreach (var claim in claims)
    {
        sw.WriteLine($"{patientId},{claim},,Claim");
    }
}