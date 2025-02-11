using System.Runtime.InteropServices;

namespace ClearImage
{
    [ComVisible(true)]
    public enum EMorphDirections
    {
        ciMorphHorz = 1,
        ciMorphVert = 2,
        ciMorphHorzAndVert = 3,
        ciMorphDiag = 4,
        ciMorphAllNeighbours = 7
    }
}