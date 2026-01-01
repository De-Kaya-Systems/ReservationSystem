namespace DeKayaServer.BlazorApp.Models;

public sealed record LoginRequest(string EmailOrUserName, string Password);