using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SourceControlSync.Domain
{
    public class ChangesCalculator : IChangesCalculator
    {
        private IEnumerable<Commit> _commits;
        private IDictionary<string, ItemChange> _items;

        public IEnumerable<ItemChange> ItemChanges
        {
            get { return _items != null ? _items.Values.ToList() : Enumerable.Empty<ItemChange>(); }
        }

        public void CalculateItemChanges(params Commit[] commits)
        {
            CalculateItemChanges(commits.AsEnumerable());
        }

        public void CalculateItemChanges(IEnumerable<Commit> commits)
        {
            _commits = commits;
            _items = new Dictionary<string, ItemChange>();

            ProcessCommits();
        }

        private void ProcessCommits()
        {
            var changes = _commits
                .OrderBy(commit => commit.Committer.Date)
                .SelectMany(commit => commit.Changes);
            foreach (var change in changes)
            {
                ProcessChange(change);
            }
        }

        private void ProcessChange(ItemChange change)
        {
            Debug.WriteLine("{0} {1}", change.ChangeType.ToString(), change.Item.Path);

            if ((change.ChangeType & ItemChangeType.Add) != 0)
            {
                AddChange(change);
            }
            else if ((change.ChangeType & ItemChangeType.Delete) != 0)
            {
                DeleteChange(change);
            }
            else if ((change.ChangeType & ItemChangeType.Edit) != 0)
            {
                EditChange(change);
            }
            else if ((change.ChangeType & ItemChangeType.Rename) != 0)
            {
                RenameChange(change);
            }
            else
            {
                // Ignore all other types
            }
        }

        private void AddChange(ItemChange change)
        {
            if (_items.ContainsKey(change.Item.Path))
            {
                ValidateExistingChangeForAdd(_items[change.Item.Path]);
                _items[change.Item.Path] = change;
            }
            else
            {
                _items.Add(change.Item.Path, change);
            }
        }

        private void EditChange(ItemChange change)
        {
            if (_items.ContainsKey(change.Item.Path))
            {
                _items[change.Item.Path] = EditExistingChange(change, _items[change.Item.Path]);
            }
            else
            {
                _items.Add(change.Item.Path, change);
            }
        }

        private void DeleteChange(ItemChange change)
        {
            if (_items.ContainsKey(change.Item.Path))
            {
                if (IsItemAdded(_items[change.Item.Path]))
                {
                    _items.Remove(change.Item.Path);
                }
                else
                {
                    _items[change.Item.Path] = change;
                }
            }
            else
            {
                _items.Add(change.Item.Path, change);
            }
        }

        private void RenameChange(ItemChange change)
        {
            AddChange(change);
        }

        private static ItemChange EditExistingChange(ItemChange newChange, ItemChange existingChange)
        {
            ValidateExistingChangeForEdit(existingChange);

            var editedExistingItem = new ItemChange()
            {
                ChangeType = existingChange.ChangeType,
                Item = newChange.Item,
                NewContent = newChange.NewContent
            };
            return editedExistingItem;
        }

        private static bool IsItemAdded(ItemChange existingChange)
        {
            return (existingChange.ChangeType & (ItemChangeType.Add | ItemChangeType.Rename)) != 0;
        }

        private static void ValidateExistingChangeForAdd(ItemChange existingItem)
        {
            if ((existingItem.ChangeType & ItemChangeType.Delete) == 0)
            {
                throw new ApplicationException("Cannot add item on an existing item");
            }
        }

        private static void ValidateExistingChangeForEdit(ItemChange existingChange)
        {
            if ((existingChange.ChangeType & ItemChangeType.Delete) != 0)
            {
                throw new ApplicationException("Cannot edit a deleted item");
            }
        }
    }
}
