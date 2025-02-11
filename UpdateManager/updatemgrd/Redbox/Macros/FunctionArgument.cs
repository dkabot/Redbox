namespace Redbox.Macros
{
    internal class FunctionArgument
    {
        private readonly int _index;
        private readonly string _name;
        private readonly object _value;
        private readonly ExpressionTokenizer.Position _beforeArgument;
        private readonly ExpressionTokenizer.Position _afterArgument;

        public FunctionArgument(
          string name,
          int index,
          object value,
          ExpressionTokenizer.Position beforeArgument,
          ExpressionTokenizer.Position afterArgument)
        {
            this._name = name;
            this._index = index;
            this._value = value;
            this._beforeArgument = beforeArgument;
            this._afterArgument = afterArgument;
        }

        public int Index => this._index;

        public string Name => this._name;

        public object Value => this._value;

        public ExpressionTokenizer.Position BeforeArgument => this._beforeArgument;

        public ExpressionTokenizer.Position AfterArgument => this._afterArgument;
    }
}
