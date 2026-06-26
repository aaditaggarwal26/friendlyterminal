namespace FriendlyTerminal.Core.Help;

public sealed record SearchHit(CommandCategory Category, CommandItem Item);

public static class CommandSearch
{
    public static IEnumerable<SearchHit> Search(IEnumerable<CommandCategory> categories, string query)
    {
        var q = query.Trim().ToLowerInvariant();
        if (q.Length == 0) yield break;

        foreach (var category in categories)
        {
            var categoryMatches = category.Name.ToLowerInvariant().Contains(q);
            foreach (var item in category.Commands)
            {
                if (categoryMatches
                    || item.Command.ToLowerInvariant().Contains(q)
                    || item.Detail.ToLowerInvariant().Contains(q)
                    || item.Keywords.ToLowerInvariant().Contains(q))
                {
                    yield return new SearchHit(category, item);
                }
            }
        }
    }
}
