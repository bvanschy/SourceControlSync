using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.Domain
{
    public class ChangesCalculator : IChangesCalculator
    {
        public IEnumerable<ItemChange> CalculateItemChanges(params Commit[] commits)
        {
            return CalculateItemChanges(commits.AsEnumerable());
        }

        public IEnumerable<ItemChange> CalculateItemChanges(IEnumerable<Commit> commits)
        {
            var items = new Dictionary<string, ItemChange>();
            foreach (var commit in commits.OrderBy(c => c.Committer.Date))
            {
                foreach (var change in commit.Changes)
                {
                    Debug.WriteLine("{0} {1}", change.ChangeType.ToString(), change.Item.Path);

                    if ((change.ChangeType & ItemChangeType.Add) != 0)
                    {
                        AddChange(items, change);
                    }
                    else if ((change.ChangeType & ItemChangeType.Delete) != 0)
                    {
                        DeleteChange(items, change);
                    }
                    else if ((change.ChangeType & ItemChangeType.Edit) != 0)
                    {
                        EditChange(items, change);
                    }
                    else if ((change.ChangeType & ItemChangeType.Rename) != 0)
                    {
                        AddChange(items, change);
                    }
                    else
                    {
                        // Ignore all other change types
                    }
                }
            }

            return items.Values.ToList();
        }

        private static void EditChange(Dictionary<string, ItemChange> items, ItemChange change)
        {
            if (items.ContainsKey(change.Item.Path))
            {
                var existingItem = items[change.Item.Path];
                if ((existingItem.ChangeType & ItemChangeType.Delete) != 0)
                {
                    throw new ApplicationException("Cannot edit a deleted item");
                }

                var editedExistingItem = new ItemChange()
                {
                    ChangeType = existingItem.ChangeType,
                    Item = change.Item,
                    NewContent = change.NewContent
                };
                items[change.Item.Path] = editedExistingItem;
            }
            else
            {
                items.Add(change.Item.Path, change);
            }
        }

        private static void DeleteChange(Dictionary<string, ItemChange> items, ItemChange change)
        {
            if (items.ContainsKey(change.Item.Path))
            {
                var existingItem = items[change.Item.Path];
                if ((existingItem.ChangeType & (ItemChangeType.Add | ItemChangeType.Rename)) != 0)
                {
                    items.Remove(change.Item.Path);
                }
                else
                {
                    items[change.Item.Path] = change;
                }
            }
            else
            {
                items.Add(change.Item.Path, change);
            }
        }

        private static void AddChange(Dictionary<string, ItemChange> items, ItemChange change)
        {
            if (items.ContainsKey(change.Item.Path))
            {
                var existingItem = items[change.Item.Path];
                if ((existingItem.ChangeType & ItemChangeType.Delete) == 0)
                {
                    throw new ApplicationException("Cannot add item on an existing item");
                }
                items[change.Item.Path] = change;
            }
            else
            {
                items.Add(change.Item.Path, change);
            }
        }
    }
}
