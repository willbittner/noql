namespace NoQL.CEP.Datastructures
{
    public class Pair<LeftType, RightType>
    {
        public LeftType Left { get; set; }

        public RightType Right { get; set; }

        public Pair(LeftType Left, RightType Right)
        {
            this.Left = Left;
            this.Right = Right;
        }
    }
}