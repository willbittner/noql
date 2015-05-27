using NoQL.CEP.Blocks;
using NoQL.CEP.Connections;
using NoQL.CEP.Datastructures;
using NoQL.CEP.RemoteHooks;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using GraphSharp.Algorithms.Layout.Simple.Hierarchical;
using System.Linq;
using System.Threading;
using System.Windows.Documents;
using GraphSharp.Controls;

namespace NoQL.CEP.NewExpressions
{
    public static class ExprTree
    {
        private static CEPExpressionTree tree = new CEPExpressionTree();
        private static CEPExpressionTreeViewModel viewmodel = new CEPExpressionTreeViewModel();
        private static int _nextComponentID = 0;
        private static LockingList<AbstractBlock> inputBlocks = new LockingList<AbstractBlock>();
        private static CEPExpressionTreeViewModel _model;
        public static CEPExpressionTreeViewModel ViewModel
        {
            get { return _model; }
        }
        public static CEPExpressionTree Tree
        {
            get
            {
                return tree;
            }
        }
        public static int NextComponentID
        {
            get
            {
                return Interlocked.Increment(ref _nextComponentID);
            }
            set
            {
                throw new Exception("Dont call this");
            }
        }

        public static void CreateComponent(INewComponent component, AbstractBlock baseBlock)
        {
            lock (tree)
            {
                if (baseBlock.ComponentID == 0)
                {
                    component.ID = NextComponentID;
                    component.InputBlock = baseBlock;
                    baseBlock.ComponentID = component.ID;
                    tree.AddVertex(baseBlock);
                    if (!inputBlocks.Where(x => x.UniqueID == baseBlock.UniqueID).Any()) inputBlocks.Add(baseBlock);
                }
                else
                {
                    component.ID = baseBlock.ComponentID;
                }
            }
        }

        public static void CreateIndependentBlock(AbstractBlock block)
        {
            block.ComponentID = NextComponentID;
            tree.AddVertex(block);
            if (!inputBlocks.Where(x => x.UniqueID == block.UniqueID).Any()) inputBlocks.Add(block);
        }

        public static AbstractBlock GetOutputBlock(int componentID)
        {
            foreach (var block in inputBlocks)
            {
                if (block.ComponentID == componentID)
                {
                    var outputBlock = SameComponentDeepestChild(block, componentID);
                    if (outputBlock == null) throw new Exception("Output block cannot be null");
                    return outputBlock;
                }
            }
            throw new Exception("No input block with that component ID was found");
        }

        public static AbstractBlock GetInputBlock(int componentID)
        {
            var inputs = inputBlocks.Where(x => x.ComponentID == componentID);
            if (!inputs.Any()) throw new Exception(" No inputblock with that componentID");
            if (inputs.Count() > 1) throw new Exception("Multiple input blocks with same component ID");
            return inputs.First();
        }

        private static void Attach<FilterType>(AbstractBlock block, Filter<FilterType> filter, INewComponent component)
        {
            if (component.ID == 0) throw new Exception("Tried to add child to a component with no ID");
            if (block.ComponentID == null || block.ComponentID == 0) block.ComponentID = component.ID;
            Connection conn = component.OutputBlock.AddChild(block, filter);
        }

        public static void Branch<FilterType>(AbstractBlock block, Filter<FilterType> filter, INewComponent component)
        {
            component.ID = 0;
            CreateComponent(component, block);
            Attach(block, filter, component);
        }

        private static AbstractBlock SameComponentDeepestChild(AbstractBlock block, int componentID)
        {
            List<AbstractBlock> blocks = new List<AbstractBlock>();
            if (!block.Children.Any()) blocks.Add(block);
            else
            {
                foreach (var childconn in block.Children)
                {
                    if (!blocks.Where(x => x.UniqueID == childconn.Destination.UniqueID).Any() && childconn.Destination.ComponentID == componentID)
                    {
                        var blockToAdd = SameComponentDeepestChild(childconn.Destination, componentID);
                        if (blockToAdd != null)
                        {
                            blocks.Add(blockToAdd);
                        }
                        else
                        {
                            if (!blocks.Where(x => x.UniqueID == childconn.Destination.UniqueID).Any()) blocks.Add(childconn.Destination);
                        }
                    }
                }
            }
            if (blocks.Count > 1) throw new Exception("Component ID: " + componentID + " has multiple output blocks with same component ID");
            if (blocks.Count == 0) blocks.Add(block);
            return blocks.First();
        }

        public static List<AbstractBlock> GetChildren(AbstractBlock parentBlock)
        {
            List<AbstractBlock> childs = new List<AbstractBlock>();
            foreach (var conn in parentBlock.Children)
            {
                childs.Add(conn.Destination);
            }
            return childs;
        }

        public static Connection<MessageType> BlockAddChild<MessageType>(AbstractBlock sourceBlock, AbstractBlock destBlock, Filter<MessageType> filter)
        {
            lock (sourceBlock.Children)
            {
                if (sourceBlock.ComponentID == null || sourceBlock.ComponentID == 0) throw new Exception("Cannot add a child to a block when the source block componentID is null or 0");
                if (filter == null) filter = new Filter<MessageType>(null);

                if (destBlock.ComponentID == null || destBlock.ComponentID == 0) destBlock.ComponentID = sourceBlock.ComponentID;
                var connection = (new Connection<MessageType> { ConnectionFilter = filter, Destination = destBlock });
                connection.Source = sourceBlock;
                tree.AddVertex(sourceBlock);
                tree.AddVertex(connection.Destination);
                tree.AddEdge(connection);
                sourceBlock.Children.Add((connection));
                tree.AddVertex(destBlock);

                tree.AddEdge(connection);
                return connection;
            }
        }
    }

        public class CEPExpressionTree : BidirectionalGraph<AbstractBlock, Connection>
    {

    }
    public class ExprTreeLayout : GraphLayout<AbstractBlock, Connection, CEPExpressionTree> { }
    public class CEPExpressionTreeViewModel : INotifyPropertyChanged {
        private string layoutAlgorithmType;
		private CEPExpressionTree graph;
        ExprTreeLayout _layout;
        public ExprTreeLayout Layout
        {
            get
            {
                if(_layout == null) _layout = new ExprTreeLayout();
                return _layout;
            }
        }
		public string LayoutAlgorithmType
		{
			get { return layoutAlgorithmType; }
			set
			{
				if (value != layoutAlgorithmType)
				{
					layoutAlgorithmType = value;
					NotifyChanged("LayoutAlgorithmType");
				}
			}
		}
      
		public CEPExpressionTree Graph
		{
			get { return graph; }
			set
			{
				if (value != graph)
				{
					graph = value;
					NotifyChanged("Graph");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
    }

}