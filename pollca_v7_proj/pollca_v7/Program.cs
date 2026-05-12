// See https://aka.ms/new-console-template for more information

using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Globalization;
using ccTalk;


string revision = "2026-05-12"; // for logging only, no auto-update mechanism

// -----------------------------------------------------------------------------
// Defaults (mirroring main.cpp)
// -----------------------------------------------------------------------------
const string DEF_PORT_NAME = "/dev/ttyUSB0";
const string DEF_CALLBACK_URL = "http://localhost:5000/coinacceptor/local";
const int DEF_CLOSING_AMOUNT_X100 = 9999999; // 99.999,99

string port = DEF_PORT_NAME;
string callbackUrl = DEF_CALLBACK_URL;
string extendedLogging = "n";
int closingAmount_x100 = DEF_CLOSING_AMOUNT_X100;
int receivedAmount_x100 = 0;

// -----------------------------------------------------------------------------
// Parse args:  <port> [callbackUrl] [extendedLogging] [closingAmount_x100]
// -----------------------------------------------------------------------------
if (args.Length < 1)
{
    Console.WriteLine("Usage: pollca_v7 <port> [callbackUrl] [extendedLogging] [closingAmount_x100]");
    Console.WriteLine("  <port>              Serial port to open, e.g. COM11 or /dev/ttyUSB0");
    Console.WriteLine("  [callbackUrl]       URL to POST coin values to (default: " + DEF_CALLBACK_URL + ")");
    Console.WriteLine("  [extendedLogging]   'y' or 'n' (default: n)");
    Console.WriteLine("  [closingAmount_x100] Stop when received total reaches this amount * 100");
    return 1;
}

port = args[0];
if (args.Length > 1) callbackUrl = args[1];
if (args.Length > 2) extendedLogging = args[2];
if (args.Length > 3 && int.TryParse(args[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var ca))
    closingAmount_x100 = ca;

// Auto-flush stdout/stderr so that output is visible immediately when the
// process is launched with redirected pipes (e.g. Python's subprocess.Popen
// with stdout=logfile). Without this, .NET block-buffers redirected output
// and the log file looks empty for long stretches.
Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });

Console.WriteLine($"FAVetronics pollca_v7 revision {revision} starting...");
Console.WriteLine($"Using port:           {port}");
Console.WriteLine($"Callback URL:         {callbackUrl}");
Console.WriteLine($"Extended logging:     {extendedLogging}");
Console.WriteLine($"Closing amount x100:  {closingAmount_x100}");

// -----------------------------------------------------------------------------
// HTTP client used for POST_Coinval
// -----------------------------------------------------------------------------
using var http = new HttpClient();
http.DefaultRequestHeaders.UserAgent.ParseAdd("ccTalkNetLinux/1.0");
// Basic auth "root:" — matches CURLOPT_USERPWD in main.cpp
var basic = Convert.ToBase64String(Encoding.ASCII.GetBytes("root:"));
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);

void PostCoinval(int amountX100)
{
    string nowIso = DateTime.Now.ToString("yyyy:MM:dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture);
    string jsonObj = $"{{\"method\":\"coinAcceptor\", \"data\":{{\"amountreceived\" : {amountX100}, \"DateTime\": \"{nowIso}\"}}}}";
    Console.WriteLine($"POST -> {callbackUrl}");
    Console.WriteLine($"This: {jsonObj}");

    try
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, callbackUrl);
        req.Content = new StringContent(jsonObj, Encoding.UTF8, "application/json");
        using var resp = http.Send(req); // synchronous, like the C++ version
        Console.WriteLine($"http code: {(int)resp.StatusCode}");
        if (extendedLogging.StartsWith("y", StringComparison.OrdinalIgnoreCase) ||
            extendedLogging.StartsWith("Y"))
        {
            var body = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Console.WriteLine($"response: {body}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"POST failed: {ex.Message}");
        Console.WriteLine($"URL: {callbackUrl}");
        Console.WriteLine($"JSON: {jsonObj}");
    }
}

// -----------------------------------------------------------------------------
// Open coin selector
// -----------------------------------------------------------------------------
var coinSelector = new SelectorComm();
coinSelector.Port = port;
var openResult = coinSelector.OpenComm();
if (openResult != CcTalkErrors.Ok)
{
    Console.WriteLine($"OpenComm failed on '{port}': {openResult} (LastError={coinSelector.LastError})");
    Console.WriteLine("On Linux, common causes:");
    Console.WriteLine("  - Your user is not in the 'dialout' group:  sudo usermod -aG dialout $USER  (then log out/in)");
    Console.WriteLine("  - The port is in use by another process (e.g. ModemManager):  sudo systemctl stop ModemManager");
    Console.WriteLine("  - Wrong device node. Check with:  ls -l /dev/ttyACM* /dev/ttyUSB*");
    return 2;
}
Console.WriteLine("Port opened.");

coinSelector.ResetDevice(30);

// Inhibit all coins first (matches existing behaviour)
var coinStatus = new SelCoinStatus[16];
for (int i = 0; i < 16; i++)
{
    coinStatus[i].Inhibit = true;
}
coinSelector.SetCoinInhibit(coinStatus);

// Read coin table
var coinValues = new CoinValue[16];
coinSelector.GetCoinValues(ref coinValues);
if (coinSelector.LastError == CcTalkErrors.Ok)
{
    for (int i = 0; i < 16; i++)
    {
        if (coinValues[i].IntValue > 0)
            Console.WriteLine($"{i + 1:d02}: {coinValues[i].Value:f2} {coinValues[i].ID}");
        else
            Console.WriteLine($"{i + 1:d02}:");
    }
}

// Signal "ready" to the server (received = 0), as the C++ reference does
PostCoinval(0);

// -----------------------------------------------------------------------------
// Main poll loop — POST every coin, stop when closing amount is reached
// -----------------------------------------------------------------------------
while (receivedAmount_x100 < closingAmount_x100)
{
    var pollResponses = new SelPollResponse[5];
    int events;

    coinSelector.PollSelector(ref pollResponses, out events);
    if (coinSelector.LastError == CcTalkErrors.Ok)
    {
        if (events > 0)
        {
            for (int i = 0; i < events; i++)
            {
                if (pollResponses[i].Status == SelPollEvent.Coin)
                {
                    var coin = coinValues[pollResponses[i].CoinIndex];
                    int coinval_x100 = (int)Math.Round(coin.Value * 100.0);
                    Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffffff}  Coin idx {pollResponses[i].CoinIndex + 1}: {coin.Value:f2} {coin.ID}");

                    PostCoinval(coinval_x100);
                    receivedAmount_x100 += coinval_x100;

                    if (receivedAmount_x100 >= closingAmount_x100)
                        break;
                }
                else
                {
                    Console.WriteLine($"{pollResponses[i].Status}");
                }
            }
        }
    }
    else
    {
        Console.WriteLine($"Poll error: {coinSelector.LastError}");
    }

    // small delay to avoid busy-looping when no events arrive
    Thread.Sleep(50);
}

Console.WriteLine($"Requested {closingAmount_x100} - Received {receivedAmount_x100}");
Console.WriteLine("bye");
return 0;
