﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Diagnostics;
using Xunit;

namespace Avalonia.Controls.TreeDataGrid.Tests
{
    public class HierarchicalTreeDataGridSourceTests
    {
        public class RowsAndCells
        {
            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public void Creates_Cells_For_Root_Models(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                AssertState(target, data, 5, sorted);
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public void Expanding_Root_Node_Creates_Child_Cells(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);
                var expander = Assert.IsType<ExpanderCell<Node>>(target.Cells[0, 0]);

                expander.IsExpanded = true;

                var expanderRow = (HierarchicalRow<Node>)expander.Row;
                AssertState(target, data, 10, sorted, expanderRow.ModelIndexPath);
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public void Collapsing_Root_Node_Removes_Child_Cells(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);
                var expander = (IExpanderCell)target.Cells[0, 0];

                expander.IsExpanded = true;
                Assert.Equal(10, target.Rows.Count);
                Assert.Equal(10, target.Cells.RowCount);

                expander.IsExpanded = false;

                AssertState(target, data, 5, sorted);
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public void Creates_Cells_For_Added_Root_Row(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                Assert.Equal(5, target.Rows.Count);
                Assert.Equal(5, target.Cells.RowCount);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                data.Add(new Node { Id = 100, Caption = "New Node 1" });

                AssertState(target, data, 6, sorted);
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public void Creates_Cells_For_Inserted_Root_Row(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                Assert.Equal(5, target.Rows.Count);
                Assert.Equal(5, target.Cells.RowCount);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                data.Insert(1, new Node { Id = 100, Caption = "New Node 1" });

                AssertState(target, data, 6, sorted);
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public void Creates_Cells_For_Added_Child_Row(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);
                var expander = (IExpanderCell)target.Cells[0, 0];

                expander.IsExpanded = true;
                Assert.Equal(10, target.Rows.Count);
                Assert.Equal(10, target.Cells.RowCount);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                var expanderRow = (HierarchicalRow<Node>)expander.Row;
                var i = expanderRow.ModelIndexPath.GetAt(0);
                data[i].Children!.Add(new Node { Id = 100, Caption = "New Node 1" });

                AssertState(target, data, 11, sorted, expanderRow.ModelIndexPath);
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public void Creates_Cells_For_Inserted_Child_Row(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);
                var expander = (IExpanderCell)target.Cells[0, 0];

                expander.IsExpanded = true;
                Assert.Equal(10, target.Rows.Count);
                Assert.Equal(10, target.Cells.RowCount);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                var expanderRow = (HierarchicalRow<Node>)expander.Row;
                var i = expanderRow.ModelIndexPath.GetAt(0);
                data[i].Children!.Insert(1, new Node { Id = 100, Caption = "New Node 1" });

                AssertState(target, data, 11, sorted, expanderRow.ModelIndexPath);
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public void Removes_Cells_For_Removed_Root_Row(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                Assert.Equal(5, target.Rows.Count);
                Assert.Equal(5, target.Cells.RowCount);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                data.RemoveAt(3);

                AssertState(target, data, 4, sorted);
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public void Unsubscribes_From_Removed_Expanded_Root_Row(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);
                var toRemove = data[3];
                var expander = GetExpander(target, toRemove);

                expander.IsExpanded = true;

                Assert.Equal(10, target.Rows.Count);
                Assert.Equal(10, target.Cells.RowCount);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                Assert.Equal(2, toRemove.PropertyChangedSubscriberCount());
                Assert.Equal(1, toRemove.Children!.CollectionChangedSubscriberCount());

                data.RemoveAt(3);

                AssertState(target, data, 4, sorted);
                Assert.Equal(0, toRemove.PropertyChangedSubscriberCount());
                Assert.Equal(0, toRemove.Children!.CollectionChangedSubscriberCount());
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public void Removes_Cells_For_Removed_Child_Row(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);
                var expander = (IExpanderCell)target.Cells[0, 0];

                expander.IsExpanded = true;
                Assert.Equal(10, target.Rows.Count);
                Assert.Equal(10, target.Cells.RowCount);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                var expanderRow = (HierarchicalRow<Node>)expander.Row;
                var i = expanderRow.ModelIndexPath.GetAt(0);
                data[i].Children!.RemoveAt(3);

                AssertState(target, data, 9, sorted, expanderRow.ModelIndexPath);
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public void Removes_Cells_For_Removed_Root_Rows_With_Expanded_Rows(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);
                var expander = (IExpanderCell)target.Cells[0, 2];

                expander.IsExpanded = true;
                Assert.Equal(10, target.Rows.Count);
                Assert.Equal(10, target.Cells.RowCount);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                data.RemoveRange(1, 3);

                AssertState(target, data, 2, sorted);
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public void Unsubscribes_From_Removed_Root_Rows_With_Expanded_Rows(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);
                var expanded = data[2];
                var expander = GetExpander(target, expanded);
                
                var toRemove = new List<Node> { data[1], data[2], data[3] };
                toRemove.AddRange(expanded.Children!);

                expander.IsExpanded = true;
                Assert.Equal(10, target.Rows.Count);
                Assert.Equal(10, target.Cells.RowCount);

                foreach (var row in toRemove)
                {
                    Assert.Equal(2, row.PropertyChangedSubscriberCount());
                }

                foreach (var row in data)
                {
                    Assert.Equal(row == data[2] ? 1 : 0, row.Children!.CollectionChangedSubscriberCount());
                }

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                data.RemoveRange(1, 3);

                foreach (var row in toRemove)
                {
                    Assert.Equal(0, row.PropertyChangedSubscriberCount());
                    Assert.Equal(0, row.Children!.CollectionChangedSubscriberCount()); 
                }

                AssertState(target, data, 2, sorted);
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public void Updates_Cells_For_Replaced_Root_Row(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                Assert.Equal(5, target.Rows.Count);
                Assert.Equal(5, target.Cells.RowCount);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                data[2] = new Node { Id = 100, Caption = "Replaced" };

                AssertState(target, data, 5, sorted);
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public void Updates_Cells_For_Moved_Root_Row(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                Assert.Equal(5, target.Rows.Count);
                Assert.Equal(5, target.Cells.RowCount);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                data.Move(2, 4);

                AssertState(target, data, 5, sorted);
            }

            [Fact]
            public void Setting_Sort_Updates_Cells()
            {
                var data = CreateData();
                var target = CreateTarget(data, false);
                var expander = (IExpanderCell)target.Cells[0, 0];

                expander.IsExpanded = true;
                Assert.Equal(10, target.Rows.Count);
                Assert.Equal(10, target.Cells.RowCount);

                target.Sort((x, y) => y.Id - x.Id);

                AssertState(target, data, 10, true, new IndexPath(0));
            }

            [Fact]
            public void Clearing_Sort_Updates_Cells()
            {
                var data = CreateData();
                var target = CreateTarget(data, true);
                var expander = (IExpanderCell)target.Cells[0, 0];

                expander.IsExpanded = true;
                Assert.Equal(10, target.Rows.Count);
                Assert.Equal(10, target.Cells.RowCount);

                target.Sort(null);

                AssertState(target, data, 10, false, new IndexPath(4));
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public void Updates_Cells_On_Property_Change(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                Assert.Equal(sorted? "Node 4" : "Node 0", target.Cells[1, 0].Value);

                data[sorted ? 4 : 0].Caption = "Modified";

                Assert.Equal("Modified", target.Cells[1, 0].Value);
            }

            [Fact]
            public void Can_Reassign_Items()
            {
                var data = CreateData();
                var target = CreateTarget(data, false);
                var cellsAddedRaised = 0;
                var cellsRemovedRaised = 0;
                var rowsAddedRaised = 0;
                var rowsRemovedRaised = 0;

                Assert.Equal(5, target.Rows.Count);
                Assert.Equal(5, target.Cells.RowCount);

                target.Cells.CollectionChanged += (s, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                        cellsAddedRaised += e.NewItems.Count;
                    else if (e.Action == NotifyCollectionChangedAction.Remove)
                        cellsRemovedRaised += e.OldItems.Count;
                };

                target.Rows.CollectionChanged += (s, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                        rowsAddedRaised += e.NewItems.Count;
                    else if (e.Action == NotifyCollectionChangedAction.Remove)
                        rowsRemovedRaised += e.OldItems.Count;
                };

                target.Items = CreateData(10);

                Assert.Equal(10, target.Rows.Count);
                Assert.Equal(10, cellsRemovedRaised);
                Assert.Equal(20, cellsAddedRaised);
                Assert.Equal(5, rowsRemovedRaised);
                Assert.Equal(10, rowsAddedRaised);
            }

            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public void Disposing_Releases_Listeners_On_Items(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);
                var expander = Assert.IsType<ExpanderCell<Node>>(target.Cells[0, 0]);
                var expanded = expander.Row.Model;

                expander.IsExpanded = true;

                Assert.Equal(10, target.Rows.Count);
                Assert.Equal(1, data.CollectionChangedSubscriberCount());
                Assert.Equal(1, expanded.Children!.CollectionChangedSubscriberCount());

                foreach (var node in data)
                {
                    Assert.Equal(2, node.PropertyChangedSubscriberCount());
                }

                foreach (var node in expanded.Children!)
                {
                    Assert.Equal(2, node.PropertyChangedSubscriberCount());
                }

                target.Dispose();

                Assert.Equal(0, data.CollectionChangedSubscriberCount());
                Assert.Equal(0, expanded.Children!.CollectionChangedSubscriberCount());

                foreach (var node in data)
                {
                    Assert.Equal(0, node.PropertyChangedSubscriberCount());
                }

                foreach (var node in expanded.Children!)
                {
                    Assert.Equal(0, node.PropertyChangedSubscriberCount());
                }
            }
        }

        public class Expansion
        {
            [Fact]
            public void Expanding_Previously_Expanded_Node_Creates_Expanded_Descendent()
            {
                var data = CreateData();
                var target = CreateTarget(data, false);

                data[0].Children![0].Children = new AvaloniaList<Node>
                {
                    new Node { Id = 100, Caption = "Grandchild" }
                };

                // Expand first root node.
                var cell0 = Assert.IsType<ExpanderCell<Node>>(target.Cells[0, 0]);
                cell0.IsExpanded = true;

                Assert.Equal(10, target.Rows.Count);

                // Expand first child node.
                var cell01 = Assert.IsType<ExpanderCell<Node>>(target.Cells[0, 1]);
                cell01.IsExpanded = true;

                // Grandchild should now be visible.
                Assert.Equal(11, target.Rows.Count);
                var cell12 = Assert.IsType<TextCell<string>>(target.Cells[1, 2]);
                Assert.Equal("Grandchild", cell12.Value);

                // Collapse root node.
                cell0.IsExpanded = false;
                Assert.Equal(5, target.Rows.Count);

                // And expand again. Grandchild should now be visible once more.
                cell0.IsExpanded = true;
                Assert.Equal(11, target.Rows.Count);
            }

            [Fact]
            public void Attempting_To_Expand_Node_That_Has_No_Children_Hides_Expander()
            {
                var data = new Node { Id = 0, Caption = "Node 0" };

                // Here we return true from hasChildren selector, but there are actually no children.
                // This may happen if calculating the children is expensive.
                var target = new HierarchicalTreeDataGridSource<Node>(data)
                {
                    Columns =
                    {
                        new HierarchicalExpanderColumn<Node>(
                            new TextColumn<Node, int>("ID", x => x.Id),
                            x => x.Children,
                            x => true),
                        new TextColumn<Node, string?>("Caption", x => x.Caption),
                    }
                };

                var expander = (IExpanderCell)target.Cells[0, 0];
                Assert.True(expander.ShowExpander);

                expander.IsExpanded = true;

                Assert.False(expander.ShowExpander);
                Assert.False(expander.IsExpanded);
            }
        }

        private static AvaloniaList<Node> CreateData(int count = 5)
        {
            var id = 0;
            var result = new AvaloniaList<Node>();

            for (var i = 0; i < count; ++i)
            {
                var node = new Node
                {
                    Id = id++,
                    Caption = $"Node {i}",
                    Children = new AvaloniaList<Node>(),
                };

                result.Add(node);

                for (var j = 0; j < 5; ++j)
                {
                    node.Children.Add(new Node
                    {
                        Id = id++,
                        Caption = $"Node {i}-{j}",
                        Children = new AvaloniaList<Node>(),
                    });
                }
            }

            return result;
        }

        private static HierarchicalTreeDataGridSource<Node> CreateTarget(IEnumerable<Node> roots, bool sorted)
        {
            var result = new HierarchicalTreeDataGridSource<Node>(roots)
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<Node>(
                        new TextColumn<Node, int>("ID", x => x.Id),
                        x => x.Children,
                        x => x.Children?.Count > 0),
                    new TextColumn<Node, string?>("Caption", x => x.Caption),
                }
            };

            if (sorted)
                result.Sort((x, y) => y.Id - x.Id);

            return result;
        }

        private static void AssertState(
            HierarchicalTreeDataGridSource<Node> target,
            IList<Node> data,
            int expectedRows,
            bool sorted,
            params IndexPath[] expanded)
        {
            Assert.Equal(2, target.Columns.Count);
            Assert.Equal(expectedRows, target.Rows.Count);
            Assert.Equal(expectedRows, target.Cells.RowCount);

            var rowIndex = 0;

            void AssertLevel(IndexPath parent, IList<Node> levelData)
            {
                var sortedData = levelData;

                if (sorted)
                {
                    var s = new List<Node>(levelData);
                    s.Sort((x, y) => y.Id - x.Id);
                    sortedData = s;
                }

                for (var i = 0; i < levelData.Count; ++i)
                {
                    var modelIndex = parent.CloneWithChildIndex(levelData.IndexOf(sortedData[i]));
                    var model = GetModel(data, modelIndex);
                    var row = Assert.IsType<HierarchicalRow<Node>>(target.Rows[rowIndex]);

                    Assert.Equal(modelIndex, row.ModelIndexPath);
                    Assert.Equal(expanded.Contains(modelIndex), row.IsExpanded);

                    var cell0 = Assert.IsType<ExpanderCell<Node>>(target.Cells[0, rowIndex]);
                    var cell1 = Assert.IsType<TextCell<string>>(target.Cells[1, rowIndex]);

                    Assert.Equal(model.Id, cell0.Value);
                    Assert.Equal(model.Children?.Count > 0, cell0.ShowExpander);
                    Assert.Equal(model.Caption, cell1.Value);

                    ++rowIndex;

                    if (row.IsExpanded)
                    {
                        Assert.NotNull(model.Children);
                        AssertLevel(modelIndex, model.Children!);
                    }
                }
            }

            AssertLevel(default, data);
        }

        private static IExpanderCell GetExpander<TModel>(
            HierarchicalTreeDataGridSource<TModel> target,
            TModel data)
                where TModel : class
        {
            for (var i = 0; i < target.Rows.Count; ++i)
            {
                if (((IRow<TModel>)target.Rows[i]).Model == data)
                {
                    return (IExpanderCell)target.Cells[0, i];
                }
            }

            throw new KeyNotFoundException();
        }

        private static Node GetModel(IList<Node> data, IndexPath path)
        {
            var depth = path.GetSize();
            Node? node = null;

            if (depth == 0)
                throw new NotSupportedException();

            for (var i = 0; i < depth; ++i)
            {
                var j = path.GetAt(i);
                node = node is null ? data[j] : node.Children![j];
            }

            return node!;
        }

        internal class Node : NotifyingBase
        {
            private int _id;
            private string? _caption;

            public int Id
            {
                get => _id;
                set => RaiseAndSetIfChanged(ref _id, value);
            }

            public string? Caption
            {
                get => _caption;
                set => RaiseAndSetIfChanged(ref _caption, value);
            }

            public AvaloniaList<Node>? Children { get; set; }
        }
    }
}
