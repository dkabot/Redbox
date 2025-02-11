namespace Redbox.Macros
{
    internal abstract class FunctionSetBase
    {
        protected FunctionSetBase(PropertyDictionary properties) => this.Properties = properties;

        protected PropertyDictionary Properties { get; set; }
    }
}
