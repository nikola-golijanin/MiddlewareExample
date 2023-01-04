var builder = WebApplication.CreateBuilder(args);

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

app.UseHttpsRedirection();

app.UseAuthorization();


// ---------app.Use()---------
// To chain multiple request delegates in our code, we can use
// the Use method. This method accepts a Func delegate as a
// parameter and returns a Task as a result
//https://localhost:5001
app.Use(async (context, next) =>
{
    Console.WriteLine($"Logic before executing the next delegate in the Use method");
    await next.Invoke();
    Console.WriteLine($"Logic after executing the next delegate in the Use method");
});


// ---------app.Map()---------
// The Map method is an extension method that accepts a path
// string as one of the parameters.
// When we provide the pathMatch string, the Map method will compare it
// to the start of the request path. If they match, the app will execute the
// branch.
// It is important to know that any middleware component that we add after
// the Map method in the pipeline won’t be executed. This is true even if we
// don’t use the Run middleware inside the branch
//https://localhost:5001/usingmapbranch
app.Map("/usingmapbranch", builder =>
{
    builder.Use(async (context, next) =>
    {
        Console.WriteLine("Map branch logic in the Use method before the next delegate");
        await next.Invoke();
        Console.WriteLine("Map branch logic in the Use method after the next delegate");
    });

    builder.Run(async context =>
    {
        Console.WriteLine($"Map branch response to the client in the Run method");
        await context.Response.WriteAsync("Hello from the map branch.");
    });
});


// ---------app.MapWhen()---------
// If we inspect the MapWhen method, we are going to see that it accepts
// two parameters.
// This method uses the result of the given predicate to branch the request
// pipeline.
// Here, if our request contains the provided query string, we execute the
// Run method by writing the response to the client. So, as we said, based
// on the predicate’s result the MapWhen method branch the request
// pipeline.
//https://localhost:5001/?testquerystring=test
app.MapWhen(context => context.Request.Query.ContainsKey("testquerystring"), builder =>
    {
        builder.Run(async context =>
        {
            await context.Response.WriteAsync("Hello from the MapWhen branch.");
        });
});


// ---------app.Run()---------
// The Run method doesn’t accept the next delegate as a parameter, so
// without it to send the execution to another component, this component
// short-circuits the request pipeline.
// One more thing to mention. We shouldn’t call the next.Invoke after we
// send the response to the client. This can cause exceptions if we try to set
// the status code or modify the headers of the response.
app.Run(async context =>
{
    Console.WriteLine($"Writing the response to the client in the Run method");
    await context.Response.WriteAsync("Hello from the middleware component.");
});

app.MapControllers();

app.Run();
