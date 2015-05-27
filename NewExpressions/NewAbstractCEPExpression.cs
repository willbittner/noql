using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dashx.CEP.Blocks;


namespace Dashx.CEP.NewExpressions
{

    internal class NewAbstractCEPExpression<InputType> : Express2<InputType>
    {



        protected AbstractBlock _startBlock;
        protected AbstractBlock _endBlock;


        private bool doessuck = true;
        private ICEPExpressionNode node = new CEPExpressionNode();


        private INewCEPExpression<object> swap;

        internal NewAbstractCEPExpression(ICEPExpressionNodeConnection connection)
        {
            connection.Attach(node);
        }

        internal NewAbstractCEPExpression(AbstractBlock start, AbstractBlock end)
        {
            _startBlock = start;
            _endBlock = end;
            node.InputBlock = start;
            node.OutputBock = end;
        }

        public NewAbstractCEPExpression<OutputType> NewExpr<OutputType>()
        {
            var ret = new NewAbstractCEPExpression<OutputType>();
            return ret;
        }

        internal NewAbstractCEPExpression()
        {

        }
        public INewCEPExpression<InputType> Express<InputType>(AbstractBlock startblock, AbstractBlock endblock)
        {
            var newExpression = new NewAbstractCEPExpression<InputType>(startblock, endblock);
            doessuck = false;
            return newExpression;
        }
        public static INewCEPExpression<InputType> StaticExpress(bool doeskk = false)
        {
            var newExpression = new NewAbstractCEPExpression<InputType>();
            return newExpression;
        }

        public static INewCEPExpression<InputType> StaticExpress<InputType>(bool doeskk = false)
        {
            var newExpression = new NewAbstractCEPExpression<InputType>();

            return newExpression;
        }
        public INewCEPExpression<InputType> Express()
        {
            var newExpression = new NewAbstractCEPExpression<InputType>();
            doessuck = true;
            return newExpression;
        }

        public static INewCEPExpression<InputType> Express<InputType>()
        {
            var expr = new NewAbstractCEPExpression<InputType>();
            //expr.Compile(block);

            return expr;
        }

        public INewCEPExpression<IDictionary<IndexType, DataType>> GroupBy<DataType, IndexType>(Func<DataType, IndexType> groupFunc)
        {
            return ExpressionFactory.GroupBy(groupFunc);
        }

        public INewCEPExpression<OutputType> Select<OutputType>(Func<InputType, OutputType> selectFunc, string name = "")
        {
            return ExpressionFactory.Select(selectFunc, name);
        }

        public INewCEPExpression<InputType> Where(Func<InputType, bool> whereFunc)
        {
            return ExpressionFactory.Where(whereFunc);
        }

        public INewCEPExpression<IEnumerable<InputType>> Window(TimeSpan span)
        {
            return ExpressionFactory.Window<InputType>(span);
        }

        public INewCEPExpression<IEnumerable<InputType>> Window(int numEvents)
        {
            return ExpressionFactory.Window<InputType>(numEvents);
        }

        public INewCEPExpression<KeyValuePair<InputType, IEnumerable<OutputType>>> Query<OutputType>(string databaseName, string indexName, Func<InputType, object> indexFunc)
        {
            return ExpressionFactory.Query<OutputType, InputType>(databaseName, indexName, indexFunc);
        }

        public INewCEPExpression<InputType> Delete(string dbName)
        {
            return ExpressionFactory.Delete<InputType>(dbName);
        }


        public INewCEPExpression<SingletonType> Split<SingletonType>()
        {
            return ExpressionFactory.Split<SingletonType>();
        }

        public INewCEPExpression<InputType> Save<InputType>(string DatabaseName)
        {
            return ExpressionFactory.Save<InputType>(DatabaseName);
        }

        public INewCEPExpression<InputType> Name<InputType>(string name)
        {
            this.InputBlock.DebugName += name + "_" + InputBlock.DebugName;
            this.OutputBlock.DebugName += name + "_" + OutputBlock.DebugName;
            return (INewCEPExpression<InputType>)this;

        }
        public INewCEPExpression<InputType> NotIn(INewCEPExpression<InputType> expr)
        {
               
            throw new NotImplementedException();
        }

        public INewCEPExpression<OutputType> Sum<OutputType>(Func<InputType, OutputType, OutputType> sumFunc)
        {
            return ExpressionFactory.Sum(sumFunc);
        }

        public INewCEPExpression<int> SumInt()
        {
            return ExpressionFactory.SumInt();
        }

        public INewCEPExpression<long> SumLong()
        {
            return ExpressionFactory.SumLong();
        }

        public INewCEPExpression<decimal> SumDecimal()
        {
            return ExpressionFactory.SumDecimal();
        }

        public INewCEPExpression<InputType> Perform(Action<InputType> action)
        {
            return ExpressionFactory.Perform(action);
        }

        public INewCEPExpression<InputType> Branch<InputType>(out INewCEPExpression<InputType> expr)
        {
            expr = Yolo.Express<InputType>();
            this.Attach(expr);
            return new NewAbstractCEPExpression<InputType>(this.InputBlock, this.OutputBlock);
            //return new NewAbstractCEPExpression<InputType>();
        }
        public INewCEPExpression<InputType> Count()
        {
            throw new NotImplementedException();
        }


        public INewCEPExpression<InputType> OnLog()
        {
            return new NewAbstractCEPExpression<InputType>(node.GetLogConnection());
        }

        public INewCEPExpression<InputType> OnError()
        {
            return new NewAbstractCEPExpression<InputType>(node.GetErrorConnection());
        }

        public INewCEPExpression<InputType> Else()
        {
            INewCEPExpression<InputType> expr = new NewAbstractCEPExpression<InputType>(node.GetElseConnection());
            return expr;
        }

        INewCEPExpression<OutputType> CompileAndReturn<OutputType>(INewCEPExpression<OutputType> expr)
        {

            //if (InputBlock.DebugName == "ExprStart") InputBlock = expr.InputBlock;
            //if (OutputBlock.DebugName == "ExprStart") OutputBlock = expr.InputBlock;
            //if (expr.InputBlock.DebugName == "ExprStart") expr.InputBlock = InputBlock;
            //if (expr.OutputBlock.DebugName == "ExprStart") expr.OutputBlock = InputBlock;
            //if (swap != null) this.Attach(swap);
            //else InputBlock = expr.OutputBlock;
            //swap = (INewCEPExpression<object>)expr;
            this.Attach(expr);
            //if ()
            return this.NewExpr<OutputType>();
            //return this;
        }
        //public void Compile(AbstractBlock block)
        //{
        //    node.Compile(block);
        //}

        public void Attach(INewComponent component)
        {
            node.Attach(component);
        }

        public void Detach(INewComponent component)
        {
            node.Attach(component);
        }


        public AbstractBlock OutputBlock
        {
            get
            {
                return node.OutputBlock;
                //  return _endBlock;
            }
            set
            {

            }
        }


        public AbstractBlock InputBlock
        {
            get
            {
                return node.InputBlock;
                // return _startBlock;
            }
            set
            {

            }
        }




        public INewCEPExpression<InputType> Where(Func<InputType, bool> whereFunc, out INewCEPExpression<InputType> elseBranch)
        {
            throw new NotImplementedException();
        }
    }
}
