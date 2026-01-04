// Chay: dotnet script GenerateHash.cs
// Hoac copy vao Program.cs tam thoi

using BCrypt.Net;

var password = "123456";
var hash = BCrypt.HashPassword(password);

Console.WriteLine("=== BCRYPT HASH ===");
Console.WriteLine($"Password: {password}");
Console.WriteLine($"Hash: {hash}");
Console.WriteLine("");
Console.WriteLine("Copy hash nay vao SampleData.sql:");
Console.WriteLine(hash);

// Test verify
var isValid = BCrypt.Verify(password, hash);
Console.WriteLine($"\nTest verify: {isValid}");
