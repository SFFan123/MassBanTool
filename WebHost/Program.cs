
using System.Text.Json;

namespace WebAPI
{
    public interface ITokenReceiver
    {
        public void ReceiveToken(string token);
    }


    public static class Program
    {
        public static int port = 23715;
        private static ITokenReceiver _instance;

        public static Task StartAsync(CancellationToken ct, ITokenReceiver instance)
        {
            _instance = instance;
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls($"http://*:{port}");

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting(); // Ensure routing middleware is added

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // Add manual routes
                endpoints.MapGet("/token", HandleGetRequest);

                endpoints.MapPost("/token", HandlePostRequest);

                // Add more manual routes as needed
            });

            return app.RunAsync(ct);
        }

        private static async Task HandlePostRequest(HttpContext context)
        {
            if (context.Request.ContentType != "application/json")
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Bad Request");
                return;
            }
            using var reader = new StreamReader(context.Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var json = JsonDocument.Parse(requestBody);
            var token = json.RootElement.GetProperty("token").GetString();

            // Use the token as needed, for example:
            _instance.ReceiveToken(token);

            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync("Success, you can close the Tab now.");
        }

        private static Task HandleGetRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync(@"
                <html>
                    <body>
                        <h1>MassBan Token Receiver</h1>
                        <input type='text' id='access_token' placeholder='Enter token here' />
                        <button onclick='postToken()'>Submit</button>
                        <script>
                            // Extract token from URL hash
                            window.onload = function() {
                                var hash = window.location.hash.split('&')[0]
                                if (hash.startsWith('#access_token')) {
                                    var token = hash.substring(1);
                                    document.getElementById('access_token').value = token.split('=')[1];
                                postToken();
                                }
                            };

                            function postToken() {
                                var token = document.getElementById('access_token').value;
                                fetch('/token', {
                                    method: 'POST',
                                    headers: {
                                        'Content-Type': 'application/json'
                                    },
                                    body: JSON.stringify({ token: token })
                                }).then(response => response.text())
                                  .then(data => alert(data));
                            }
                        </script>
                    </body>
                </html>");
        }
    }
}
