namespace Redbox.Macros
{
    internal class FunctionArgument
    {
        public FunctionArgument(
            string name,
            int index,
            object value,
            ExpressionTokenizer.Position beforeArgument,
            ExpressionTokenizer.Position afterArgument)
        {
            Name = name;
            Index = index;
            Value = value;
            BeforeArgument = beforeArgument;
            AfterArgument = afterArgument;
        }

        public int Index { get; }

        public string Name { get; }

        public object Value { get; }

        public ExpressionTokenizer.Position BeforeArgument { get; }

        public ExpressionTokenizer.Position AfterArgument { get; }
    }
}