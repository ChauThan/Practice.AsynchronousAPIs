using AsyncProductAPI.Data;
using AsyncProductAPI.Dtos;
using AsyncProductAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source=RequestDB.db"));

var app = builder.Build();

app.UseHttpsRedirection();

// start endpoint
app.MapPost("api/v1/products", async (AppDbContext context, ListingRequest request) =>
{
    if (request is null)
    {
        return Results.BadRequest();
    }

    request.RequestStatus = "ACCEPT";
    request.EstimatedCompletionTime = DateTimeOffset.Now.AddDays(1).ToString();

    await context.ListingRequests.AddAsync(request);
    await context.SaveChangesAsync();

    return Results.Accepted($"api/v1/productstatus/{request.RequestId}", request);
});

app.MapGet("api/v1/productstatus/{requestId}", async (AppDbContext context, HttpRequest request, string requestId) =>
{
    var listingRequest = await context.ListingRequests.FirstOrDefaultAsync(lr => lr.RequestId == requestId);
    if (listingRequest is null)
    {
        return Results.NotFound();
    }

    var status = new ListingStatus
    {
        RequestStatus = listingRequest.RequestStatus,
        ResourceURL = string.Empty
    };

    if(status.RequestStatus!.ToUpper() == "COMPLETE")
    {
        status.ResourceURL = $"{request.Scheme}://{request.Host}/api/v1/products/{Guid.NewGuid().ToString()}";

        return Results.Redirect(status.ResourceURL);
    }

    status.EstimatedCompletionTime = DateTimeOffset.Now.AddDays(1).ToString();
    return Results.Ok(status);
});

// Final Endpoint
app.MapGet("api/v1/products/{requestId}", (string requestId) => {
    return Results.Ok("This is where you would pass back the final result");
});

app.Run();

