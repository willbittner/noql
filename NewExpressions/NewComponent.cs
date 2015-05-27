using NoQL.CEP.Blocks;
using NoQL.CEP.Connections;
using System;

namespace NoQL.CEP.NewExpressions
{
    public class NewComponent : INewComponent
    {
        private BaseInputAdapter adapter;

        public NewComponent()
        {
            Init(null);
        }

        private void Init(string name)
        {
            adapter = new BaseInputAdapter();
            LambdaBlock<object> b = Express2.BlockFactory.CreateLambdaBlock<object>(x => true, name);
            ExprTree.CreateComponent(this, b);
            adapter.Attach(this);
        }

        public NewComponent(string name)
        {
            Init(name);
        }

        private AbstractBlock _inputBlock;

        public AbstractBlock OutputBlock
        {
            get { return ExprTree.GetOutputBlock(ID); }
            set
            {
                if (ID == 0) throw new Exception("Bad juju, you never set OuputBlock");
            }
        }

        public AbstractBlock InputBlock
        {
            get { return ExprTree.GetInputBlock(ID); }
            set
            {
                if (ID == 0) throw new Exception("Bad juju Don't Set Inputblock until you set the Components ID");
                if (_inputBlock != null && _inputBlock.ComponentID == value.ComponentID) throw new Exception("InputBlock is already set, you can't set it again unless you have a new componentID");
                _inputBlock = value;
            }
        }

        public void Attach(INewComponent component)
        {
            OutputBlock.AddChild(component.InputBlock, new Filter<object>(null));
        }

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

        private int _id = 0;

        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
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

        public void Detach(INewComponent component)
        {
            throw new NotImplementedException();
        }

        public void Send(object data)
        {
            adapter.Send(data);
        }

        public void OnReceive<T>(Action<T> recvFunc)
        {
            BaseOutputAdapter<T> objectOutAdapter = new BaseOutputAdapter<T>();
            objectOutAdapter.SetFunction(recvFunc, this);
        }
    }
}