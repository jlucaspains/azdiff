using Newtonsoft.Json;

namespace azdiff;

class BaseDiffCommand
{
    protected (List<ReplaceText>? result, int resultCode) PrepareReplaceStrings(FileInfo? replaceStringsFile)
    {
        List<ReplaceText>? replaceStrings = [];

        if (replaceStringsFile != null)
        {
            if (!replaceStringsFile.Exists)
            {
                Utilities.WriteError("Replace strings file does not exist.");
                return (replaceStrings, 4);
            }

            replaceStrings = GetReplaceStrings(replaceStringsFile);

            if (replaceStrings.Count == 0)
            {
                Utilities.WriteError("Replace strings file is invalid.");
                return (replaceStrings, 5);
            }
        }

        return (replaceStrings, 0);
    }

    
    protected List<ReplaceText> GetReplaceStrings(FileInfo replaceStringsFile)
    {
        try
        {
            var replaceStringsContent = File.ReadAllText(replaceStringsFile.FullName);

            return JsonConvert.DeserializeObject<List<ReplaceText>>(replaceStringsContent)
                ?? [];
        }
        catch
        {
            return [];
        }
    }

    protected static async Task WriteResultToFiles(IEnumerable<DiffResult> diffResultItems, DirectoryInfo outputFolder)
    {
        foreach (var item in diffResultItems)
        {
            var prefix = item.DiffType switch
            {
                DiffType.Diff => "diff",
                DiffType.MissingOnTarget => "new",
                DiffType.ExtraOnTarget => "extra",
                _ => throw new NotImplementedException(),
            };

            await File.WriteAllTextAsync($"{outputFolder.FullName}/{prefix}_{item.Name}.diff", item.Result);
        }
    }
}