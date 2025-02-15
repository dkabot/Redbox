namespace Redbox.HAL.Common.GUI.Functions
{
    public class MoveHelperResult
    {
        public readonly string Status;

        internal MoveHelperResult(string msg)
        {
            Status = msg;
            Limits = new string[0];
        }

        public string[] Limits { get; internal set; }

        public bool MoveOk => Status == "SUCCESS";
    }
}