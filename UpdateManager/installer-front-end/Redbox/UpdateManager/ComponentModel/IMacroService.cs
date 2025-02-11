namespace Redbox.UpdateManager.ComponentModel
{
    internal interface IMacroService
    {
        string ExpandProperties(string input);

        string this[string name] { get; set; }
    }
}
