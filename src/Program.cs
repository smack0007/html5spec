using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

const string Html5SpecUrl = "https://github.com/whatwg/html/blob/master/source?raw=true";
const string StartCodeBlock = "<code class=\"idl\">";
const string EndCodeBlock = "</code>";

Main(args);

int Main(string[] args)
{
    string specPath = Path.GetFullPath("html5.spec");

    Console.WriteLine($"Looking for html5.spec in path '{specPath}'...");

    if (!File.Exists(specPath))
    {
        Console.WriteLine($"'{specPath}' does not exist. Downloading...");

        var client = new WebClient();
        client.DownloadFile(Html5SpecUrl, specPath);
    }

    if (!File.Exists(specPath))
    {
        Console.WriteLine($"'{specPath}' still does not exist. Exiting...");
        return 1;
    }

    var codeBlocks = ParseSpec(specPath);

    // Remove the first code block as it's just an "Example".
    codeBlocks = codeBlocks.Skip(1);

    codeBlocks = codeBlocks.Select(x => RemoveHtml(x));

    File.WriteAllText("html5.idl", string.Join(Environment.NewLine, codeBlocks));

    return 0;
}

IEnumerable<string> ParseSpec(string specPath)
{
    var spec = File.ReadAllText(specPath);

    var startIndex = 0;
    while ((startIndex = spec.IndexOf(StartCodeBlock, startIndex)) != -1)
    {
        var endIndex = spec.IndexOf(EndCodeBlock, startIndex);

        if (endIndex == -1)
            break;

        var codeBlock = spec.Substring(startIndex + StartCodeBlock.Length, endIndex - startIndex - StartCodeBlock.Length);

        startIndex = endIndex + EndCodeBlock.Length;

        yield return codeBlock;
    }
}

string RemoveHtml(string codeBlock)
{
    return Regex.Replace(codeBlock, "<.*?>", string.Empty);
}