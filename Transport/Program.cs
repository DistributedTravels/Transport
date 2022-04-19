using Transport;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var manager = new EventManager("example");
app.Run();