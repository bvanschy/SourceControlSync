using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SourceControlSync.Domain.Tests
{
    [TestClass]
    public class ChangesCalculatorTests
    {
        [TestMethod]
        public void AddAnItem()
        {
            var change = CreateChange(ItemChangeType.Add, "/index.html");
            var commit = CreateCommit(0, change);
            var converter = CreateChangesCalculator();

            converter.CalculateItemChanges(commit);

            AssertCalculatedChanges(converter.ItemChanges, change);
        }

        [TestMethod]
        public void EditAnItem()
        {
            var change = CreateChange(ItemChangeType.Edit, "/index.html");
            var commit = CreateCommit(0, change);
            var converter = CreateChangesCalculator();

            converter.CalculateItemChanges(commit);

            AssertCalculatedChanges(converter.ItemChanges, change);
        }

        [TestMethod]
        public void AddThenEditAnItem()
        {
            var changeAdd = CreateChange(ItemChangeType.Add, "/index.html");
            var commitAdd = CreateCommit(0, changeAdd);
            var changeEdit = CreateChange(ItemChangeType.Edit, "/index.html");
            var commitEdit = CreateCommit(1, changeEdit);
            var converter = CreateChangesCalculator();

            converter.CalculateItemChanges(commitAdd, commitEdit);

            AssertCalculatedChanges(converter.ItemChanges, new ItemChange()
            {
                ChangeType = ItemChangeType.Add,
                Item = changeEdit.Item,
                NewContent = changeEdit.NewContent
            });
        }

        [TestMethod]
        public void DeleteAnItem()
        {
            var change = CreateChange(ItemChangeType.Delete, "/index.html");
            var commit = CreateCommit(0, change);
            var converter = CreateChangesCalculator();

            converter.CalculateItemChanges(commit);

            AssertCalculatedChanges(converter.ItemChanges, change);
        }

        [TestMethod]
        public void AddThenDeleteAnItem()
        {
            var changeAdd = CreateChange(ItemChangeType.Add, "/index.html");
            var commitAdd = CreateCommit(0, changeAdd);
            var changeDelete = CreateChange(ItemChangeType.Delete, "/index.html");
            var commitDelete = CreateCommit(1, changeDelete);
            var converter = CreateChangesCalculator();

            converter.CalculateItemChanges(commitAdd, commitDelete);

            AssertCalculatedChanges(converter.ItemChanges);
        }

        [TestMethod]
        public void EditThenDeleteAnItem()
        {
            var changeEdit = CreateChange(ItemChangeType.Edit, "/index.html");
            var commitEdit = CreateCommit(0, changeEdit);
            var changeDelete = CreateChange(ItemChangeType.Delete, "/index.html");
            var commitDelete = CreateCommit(1, changeDelete);
            var converter = CreateChangesCalculator();

            converter.CalculateItemChanges(commitEdit, commitDelete);

            AssertCalculatedChanges(converter.ItemChanges, changeDelete);
        }

        [TestMethod]
        public void AddThenEditThenDeleteAnItem()
        {
            var changeAdd = CreateChange(ItemChangeType.Add, "/index.html");
            var commitAdd = CreateCommit(0, changeAdd);
            var changeEdit = CreateChange(ItemChangeType.Edit, "/index.html");
            var commitEdit = CreateCommit(1, changeEdit);
            var changeDelete = CreateChange(ItemChangeType.Delete, "/index.html");
            var commitDelete = CreateCommit(2, changeDelete);
            var converter = CreateChangesCalculator();

            converter.CalculateItemChanges(commitAdd, commitEdit, commitDelete);

            AssertCalculatedChanges(converter.ItemChanges);
        }

        [TestMethod]
        public void DeleteThenAddAnItem()
        {
            var changeDelete = CreateChange(ItemChangeType.Delete, "/index.html");
            var commitDelete = CreateCommit(0, changeDelete);
            var changeAdd = CreateChange(ItemChangeType.Add, "/index.html");
            var commitAdd = CreateCommit(1, changeAdd);
            var converter = CreateChangesCalculator();

            converter.CalculateItemChanges(commitDelete, commitAdd);

            AssertCalculatedChanges(converter.ItemChanges, changeAdd);
        }

        [TestMethod]
        public void RenameAnItem()
        {
            var changeRename = CreateChange(ItemChangeType.Rename, "/index.html");
            var changeDelete = CreateChange(ItemChangeType.Delete | ItemChangeType.SourceRename, "/index2.html");
            var commit = CreateCommit(0, changeRename, changeDelete);
            var converter = CreateChangesCalculator();

            converter.CalculateItemChanges(commit);

            AssertCalculatedChanges(converter.ItemChanges, changeRename, changeDelete);
        }

        [TestMethod]
        public void RenameAndEditAnItem()
        {
            var changeRename = CreateChange(ItemChangeType.Edit | ItemChangeType.Rename, "/index.html");
            var changeDelete = CreateChange(ItemChangeType.Delete | ItemChangeType.SourceRename, "/index2.html");
            var commit = CreateCommit(0, changeRename, changeDelete);
            var converter = CreateChangesCalculator();

            converter.CalculateItemChanges(commit);

            AssertCalculatedChanges(converter.ItemChanges, changeRename, changeDelete);
        }

        [TestMethod]
        public void RenameThenDeleteAnItem()
        {
            var changeRename = CreateChange(ItemChangeType.Rename, "/index.html");
            var changeDelete2 = CreateChange(ItemChangeType.Delete | ItemChangeType.SourceRename, "/index2.html");
            var commitRename = CreateCommit(0, changeRename, changeDelete2);
            var changeDelete = CreateChange(ItemChangeType.Delete, "/index.html");
            var commitDelete = CreateCommit(1, changeDelete);
            var converter = CreateChangesCalculator();

            converter.CalculateItemChanges(commitRename, commitDelete);

            AssertCalculatedChanges(converter.ItemChanges, changeDelete2);
        }

        [TestMethod]
        public void RenameAndEditThenDeleteAnItem()
        {
            var changeRename = CreateChange(ItemChangeType.Edit | ItemChangeType.Rename, "/index.html");
            var changeDelete2 = CreateChange(ItemChangeType.Delete | ItemChangeType.SourceRename, "/index2.html");
            var commitRename = CreateCommit(0, changeRename, changeDelete2);
            var changeDelete = CreateChange(ItemChangeType.Delete, "/index.html");
            var commitDelete = CreateCommit(1, changeDelete);
            var converter = CreateChangesCalculator();

            converter.CalculateItemChanges(commitRename, commitDelete);

            AssertCalculatedChanges(converter.ItemChanges, changeDelete2);
        }

        [TestMethod]
        public void RenameThenAddAnItem()
        {
            var changeRename = CreateChange(ItemChangeType.Rename, "/index.html");
            var changeDelete2 = CreateChange(ItemChangeType.Delete | ItemChangeType.SourceRename, "/index2.html");
            var commitRename = CreateCommit(0, changeRename, changeDelete2);
            var changeAdd = CreateChange(ItemChangeType.Add, "/index2.html");
            var commitAdd = CreateCommit(1, changeAdd);
            var converter = CreateChangesCalculator();

            converter.CalculateItemChanges(commitRename, commitAdd);

            AssertCalculatedChanges(converter.ItemChanges, changeRename, changeAdd);
        }

        private ItemChange CreateChange(ItemChangeType changeType, string path)
        {
            return new ItemChange()
            {
                Item = new Item()
                {
                    ContentMetadata = new FileContentMetadata(),
                    Path = path
                },
                ChangeType = changeType,
                NewContent = new ItemContent()
            };
        }

        private static Commit CreateCommit(int minute, params ItemChange[] changes)
        {
            return new Commit()
            {
                Committer = new UserDate() { Date = new DateTime(2015, 8, 16, 4, minute, 13) },
                Changes = changes
            };
        }

        private static ChangesCalculator CreateChangesCalculator()
        {
            return new ChangesCalculator();
        }

        private static void AssertCalculatedChanges(IEnumerable<ItemChange> actual, params ItemChange[] expected)
        {
            Assert.AreEqual(actual.Count(), expected.Length);

            var expectedChanges = expected.ToDictionary(c => c.Item.Path);
            foreach (var actualChange in actual) 
            {
                Assert.IsTrue(expectedChanges.ContainsKey(actualChange.Item.Path));

                var expectedChange = expectedChanges[actualChange.Item.Path];
                Assert.AreEqual(expectedChange.Item.Path, actualChange.Item.Path);
                Assert.AreEqual(expectedChange.ChangeType, actualChange.ChangeType);
                Assert.AreSame(expectedChange.Item.ContentMetadata, actualChange.Item.ContentMetadata);
                Assert.AreSame(expectedChange.NewContent, actualChange.NewContent);
            };
        }
    }
}
