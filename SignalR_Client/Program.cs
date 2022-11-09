using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
using TaskTracker.Core;
using TaskTracker_BL.Services;

var url = "https://localhost:5001/chat";

//var connection = new HubConnectionBuilder()
//    .WithUrl(url)
//    .WithAutomaticReconnect()
//    .Build();

var connection = new HubConnectionBuilder()
    .WithUrl(url, options =>
    {
        //options.AccessTokenProvider = async () => await Login();
    })
    .WithAutomaticReconnect()
    .Build();

connection.ServerTimeout = TimeSpan.FromMinutes(3);

connection.On<MessageSnapshot>(nameof(ISignalRClient.GetMessage), async (message) =>
    {
        PrintMessage(message);
    }
);

await connection.StartAsync();

string? input;
string? nickname;
string? color;

do
{
    nickname = GetNickname();
    color = GetColor();
}
while (!await SetNicknameAndColor(nickname, color));

await PrintRecentMessages(nickname, 15);

do
{
    input = GetInputMessage();

    if (input == "/menu")
    {
        CallMenuAsync(nickname);
    }
    else
    {
        await connection.InvokeAsync(nameof(ISignalRServer.SendMesasageToAll), input);
    }
}
while (!string.IsNullOrEmpty(input));

async Task CallMenuAsync(string userNickname)
{
    int menuNumber;
    do
    {
        Console.Clear();
        Console.WriteLine("1 - Send personal message\n2 - Change nickname\n3 - Set text color\n4 - Send message to all");
    } while (!int.TryParse(Console.ReadLine(), out menuNumber));

    switch (menuNumber)
    {
        case 1:
            string nickname = GetNickname();
            string message = GetInputMessage();
            SendPersonalMessage(nickname, message);
            break;
        case 2:
            do
            {
                nickname = GetNickname();
            }
            while (!await ChangeNickname(nickname));
            break;
        case 3:
            string color;
            do
            {
                color = GetColor();

            } while (!await SetMessageColor(color));
            break;
    }
    Console.Clear();
    await PrintRecentMessages(userNickname, 50);
}

async Task SendPersonalMessage(string nickname, string message)
{
    await connection.InvokeAsync(nameof(ISignalRServer.SendPersonalMessage), nickname, message);
}


//async Task<IEnumerable<MessageSnapshot>> GetRecent(int count)
//{
//    return await connection.InvokeAsync<IEnumerable<MessageSnapshot>>(nameof(ISignalRServer.GetRecent), count);
//}

async Task PrintRecentMessages(string nickname, int count)
{
    var messages = await connection.InvokeAsync<IEnumerable<MessageSnapshot>>(nameof(ISignalRServer.GetRecent), count);
    foreach (var message in messages)
    {
        PrintMessage(message);
    }
}

string? GetInputMessage()
{
    return Console.ReadLine();
}

string? GetNickname()
{
    Console.WriteLine("Enter nickname");
    return Console.ReadLine();
}

string? GetColor()
{
    Console.WriteLine("Enter color");
    return Console.ReadLine();
}

async Task<bool> SetNicknameAndColor(string nickname, string color)
{
    var response = await connection.InvokeAsync<bool>(nameof(ISignalRServer.SetNicknameAndColor), nickname, color);
    SetConsoleColor(color);
    Console.Clear();
    return response;
}

async Task<bool> ChangeNickname(string nickname)
{
    string color = Console.ForegroundColor.ToString();
    return await connection.InvokeAsync<bool>(nameof(ISignalRServer.ChangeNickname), nickname);
}

async Task<bool> SetMessageColor(string color)
{
    bool response = await connection.InvokeAsync<bool>(nameof(ISignalRServer.SetMessageColor), color);
    SetConsoleColor(color);
    return response;
}

async Task SetConsoleColor(string color)
{
    object textColor = new object();

    if (Enum.TryParse(typeof(ConsoleColor), color, out textColor))
    {
        Console.ForegroundColor = (ConsoleColor)textColor;
        return;
    }

    Console.ForegroundColor = ConsoleColor.White;
}

async void PrintMessage(MessageSnapshot messageSnapshot)
{
    var current = Console.ForegroundColor;
    await SetConsoleColor(messageSnapshot.SenderUserInfo.MessageColor);
    if (messageSnapshot.IsPersonal)
    {
        Console.WriteLine($"[P] {messageSnapshot.SentDate}|{messageSnapshot.SenderUserInfo.Nickname}: {messageSnapshot.Message}");
    }
    else
    {
        Console.WriteLine($"{messageSnapshot.SentDate}|{messageSnapshot.SenderUserInfo.Nickname}: {messageSnapshot.Message}");
    }
    Console.ForegroundColor = current;
}
async Task<string> Login()
{
    Console.WriteLine("Enter login");
    string? userLogin = Console.ReadLine();
    Console.WriteLine("Enter pasword");
    string? password = Console.ReadLine();

    return await connection!.InvokeAsync<string>(nameof(ISignalRServer.Login), userLogin, password);
}

