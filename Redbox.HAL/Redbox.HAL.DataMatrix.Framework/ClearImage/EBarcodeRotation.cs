using System.Runtime.InteropServices;

namespace ClearImage
{
    [ComVisible(true)]
    public enum EBarcodeRotation
    {
        cibRotNone = 1,
        cibRotUpsideDown = 2,
        cibRotLeft = 3,
        cibRotRight = 4,
        cibRotDiag = 5
    }
}