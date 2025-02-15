namespace Redbox.Macros
{
    public abstract class FunctionSetBase
    {
        protected FunctionSetBase(PropertyDictionary properties)
        {
            Properties = properties;
        }

        protected PropertyDictionary Properties { get; set; }
    }
}