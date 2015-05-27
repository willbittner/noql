using NoQL.CEP.Blocks;
using System;

namespace NoQL.CEP.NewExpressions
{
    public class BaseOutputAdapter<DataType> : INewComponent
    {
        private AbstractBlock exprblock;
        private Action<DataType> RecvFunction;
        private INewCEPExpression<DataType> mainExprFunc;

        public BaseOutputAdapter(string name = "input_base")
        {
            Init(name);
        }

        public BaseOutputAdapter(Action<DataType> func, INewComponent fromComp, string name = "CompactOutputAdapter")
        {
            SetFunction(func, fromComp);
        }

        public void SetFunction(Action<DataType> func, INewComponent comp)
        {
            RecvFunction = func;
            if (mainExprFunc == null)
            {
                mainExprFunc = Yolo.Express<DataType>().Name("OutputAdapterFunc").Perform(x => func(x));
            }
            else mainExprFunc.Perform(x => func(x));
            comp.Attach(mainExprFunc);
        }

        private void Init(string name)
        {
            exprblock = Express2.BlockFactory.CreateLambdaBlock<object>(x => true, name);
            ExprTree.CreateComponent(this, exprblock);
        }

        public void Send(object obj)
        {
            throw new NotImplementedException("Can't send data to output blocks");
        }

        public void Receive(Action<object> recvFunc)
        {
        }

        #region INewInputAdapter Members

        public void ReceiveFrom(INewComponent comp)
        {
            if (mainExprFunc == null) throw new Exception("main expr cannot be null");
            comp.Attach(mainExprFunc);
        }

        public void Attach(INewComponent component)
        {
            //exprblock.AddChild<object>(component.InputBlock, null);
        }

        public void Detach(INewComponent component)
        {
            //exprblock.RemoveChild(component.InputBlock);
        }

        public AbstractBlock OutputBlock
        {
            get { throw new Exception("Output adapters dont ahve an Inputblock"); }
            set { throw new NotImplementedException(); }
        }

        public AbstractBlock InputBlock
        {
            get { return exprblock; }
            set { }
        }

        #endregion INewInputAdapter Members

        private string _name;

        public string ComponentName
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        private int _ID = 0;

        public int ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
            }
        }

        public int CepID
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void OnReceive<T>(Action<T> recvFunc)
        {
            throw new Exception("Use recv from");
        }
    }

    public class BaseInputAdapter : INewComponent
    {
        private AbstractBlock exprblock;

        public BaseInputAdapter(string name = "input_base")
        {
            Init(name);
        }

        public static void QuickSendData(object data, INewComponent comp)
        {
            comp.Send(data);
        }

        private void Init(string name)
        {
            exprblock = Express2.BlockFactory.CreateLambdaBlock<object>(x => true, name);
            ExprTree.CreateComponent(this, exprblock);
        }

        public void Send(object obj)
        {
            exprblock.Accept(obj);
        }

        #region INewInputAdapter Members

        public void Attach(INewComponent component)
        {
            exprblock.AddChild<object>(component.InputBlock, null);
        }

        public void Detach(INewComponent component)
        {
            exprblock.RemoveChild(component.InputBlock);
        }

        public AbstractBlock OutputBlock
        {
            get { return exprblock; }
            set { throw new NotImplementedException(); }
        }

        public AbstractBlock InputBlock
        {
            get { throw new Exception("Input adapters dont ahve an Inputblock"); }
            set { }
        }

        #endregion INewInputAdapter Members

        private string _name;

        public string ComponentName
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        private int _ID = 0;

        public int ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
            }
        }

        public int CepID
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void OnReceive<T>(Action<T> recvFunc)
        {
            throw new NotImplementedException();
        }
    }
}