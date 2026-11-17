using Xunit;
using FluentAssertions;
using Library.Api;
using System.IO.Compression;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
[assembly: CollectionBehavior(DisableTestParallelization = true)]
public class UploadTests
{
    public static byte[] Archive(string name, byte[] body)
    {
        using var data = new MemoryStream();
        using (var zip = new ZipArchive(data, ZipArchiveMode.Create, true))
            using (var stream = zip.CreateEntry(name).Open()) stream.Write(body);
        return data.ToArray();
    }
    static readonly byte[] Pdf = "%PDF-1.7\n%%EOF"u8.ToArray();
    [Theory]
    [InlineData("../escape.pdf")]
    [InlineData("/tmp/escape.pdf")]
    [InlineData("C:\\escape.pdf")]
    [InlineData("a/../../escape.pdf")]
    [InlineData("a\\escape.pdf")]
    [InlineData("a/../escape.pdf")]
    public void RejectEscape(string name)
    {
        Action act = () => UploadGuard.SafePath(Path.GetTempPath(), name);
        act.Should().Throw<InvalidDataException>();
    }
    [Fact] public void PdfAccepted() => UploadGuard.ValidatePdf(Pdf);
    [Fact] public void ExecutableRejected()
    {
        Action act = () => UploadGuard.ValidatePdf("MZ executable"u8.ToArray());
        act.Should().Throw<InvalidDataException>();
    }
    [Fact] public void EpubAccepted()
    {
        using var data = new MemoryStream();
        using (var zip = new ZipArchive(data, ZipArchiveMode.Create, true)) {
            using (var w = new StreamWriter(zip.CreateEntry("mimetype").Open())) w.Write("application/epub+zip");
            using (var w = new StreamWriter(zip.CreateEntry("META-INF/container.xml").Open())) w.Write("<container/>");
        }
        UploadGuard.ValidateEpub(data.ToArray());
    }
    [Theory]
    [InlineData(50, true)]
    [InlineData(51, false)]
    [InlineData(0, false)]
    public void EntryLimit(int count, bool accepted)
    {
        using var data = new MemoryStream();
        using (var zip = new ZipArchive(data, ZipArchiveMode.Create, true))
            for (int i = 0; i < count; i++)
                using (var s = zip.CreateEntry($"{i}.pdf").Open()) s.Write(Pdf);
        data.Position = 0;
        Action act = () => UploadGuard.ReadArchive(data, Path.GetTempPath());
        if (accepted) act.Should().NotThrow(); else act.Should().Throw<InvalidDataException>();
    }
    [Theory]
    [InlineData(10485760, true)]
    [InlineData(10485761, false)]
    public void ExpandedSizeLimit(int size, bool accepted)
    {
        var bytes = new byte[size]; Pdf.CopyTo(bytes, 0);
        using var data = new MemoryStream(Archive("book.pdf", bytes));
        Action act = () => UploadGuard.ReadArchive(data, Path.GetTempPath());
        if (accepted) act.Should().NotThrow(); else act.Should().Throw<InvalidDataException>();
    }
    [Theory]
    [InlineData("../escape.pdf", "librarian", "Book", 400)]
    [InlineData("book.exe", "librarian", "Book", 400)]
    [InlineData("book.pdf", "reader", "Book", 403)]
    [InlineData("book.pdf", "", "Book", 401)]
    [InlineData("book.pdf", "librarian", "", 400)]
    [InlineData("book.pdf", "librarian", "Book", 202)]
    public async Task UploadContract(string name, string role, string title, int status)
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Demo-User", role);
        client.DefaultRequestHeaders.Add("X-Book-Title", title);
        using var response = await client.PostAsync("/api/v1/uploads", new ByteArrayContent(Archive(name, Pdf)));
        ((int)response.StatusCode).Should().Be(status);
    }
    [Fact] public async Task OldClientStillReadsV1()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        using var json = JsonDocument.Parse(await client.GetStringAsync("/api/v1/books"));
        json.RootElement[0].GetProperty("title").GetString().Should().Be("Основы C#");
        json.RootElement[0].GetProperty("id").GetInt32().Should().Be(1);
        json.RootElement[0].TryGetProperty("format", out _).Should().BeFalse();
        using var v2 = JsonDocument.Parse(await client.GetStringAsync("/api/v2/books"));
        v2.RootElement[0].GetProperty("format").GetString().Should().Be("PDF");
    }
    [Fact] public async Task QueryIsParameterized()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        string response = await client.GetStringAsync("/api/v2/search?title=" + Uri.EscapeDataString("' OR 1=1 --"));
        response.Should().Be("[]");
    }
    [Fact] public async Task RateLimitReturns429()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Demo-User", "librarian");
        client.DefaultRequestHeaders.Add("X-Book-Title", "Book");
        HttpStatusCode last = 0;
        for (int i = 0; i < 31; i++) {
            using var r = await client.PostAsync("/api/v1/uploads", new ByteArrayContent([0]));
            last = r.StatusCode;
        }
        ((int)last).Should().Be(429);
    }
}
