using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage
{
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("CB149079-A739-4178-A4DA-BACA546C3728")]
    [ComVisible(true)]
    [ComImport]
    public class CiRepairClass : ICiRepair, CiRepair
    {
        [DispId(1)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void FaxStandardToFine();

        [DispId(2)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void FaxRemoveBlankLines();

        [DispId(3)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void FaxRemoveHeader();

        [DispId(36)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern double AutoDeskew();

        [DispId(37)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern EPageRotation AutoRotate();

        [DispId(39)]
        public virtual extern CiImage Image
        {
            [DispId(39)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
            [DispId(39)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: MarshalAs(UnmanagedType.Interface)]
            [param: In]
            set;
        }

        [DispId(16)]
        public virtual extern int pMinLineLength
        {
            [DispId(16)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(16)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(17)]
        public virtual extern int pMaxLineGap
        {
            [DispId(17)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(17)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(18)]
        public virtual extern double pMaxLineAngle
        {
            [DispId(18)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(18)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(19)]
        public virtual extern ELineCurvature pLineCurvature
        {
            [DispId(19)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(19)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(20)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void CleanBorders();

        [DispId(21)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void RemovePunchHoles();

        [DispId(22)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void RemoveHalftone();

        [DispId(23)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void SmoothCharacters([In] ESmoothType Type);

        [DispId(24)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void CleanNoise([In] int NoiseSize);

        [DispId(25)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void AutoInvertImage([In] int Threshold);

        [DispId(26)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void AutoInvertBlocks([In] int MinWidth, [In] int MinHeight);

        [DispId(29)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void ReconstructLines([In] ELineDirection Direction);

        [DispId(30)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void AutoCrop(
            [In] int NewLeftMargin,
            [In] int NewTopMargin,
            [In] int NewRightMargin,
            [In] int NewBottomMargin);

        [DispId(31)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void AutoRegister([In] int NewLeftMargin, [In] int NewTopMargin);

        [DispId(40)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void DeleteLines([In] ELineDirection Direction, [In] EBoolean bRepair = EBoolean.ciTrue);

        [DispId(41)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void BorderExtract(
            [In] EBorderExtractFlags Flags = EBorderExtractFlags.ciBexBorderDeskewCrop,
            [In] EBorderExtractAlgorithm Algorithm = EBorderExtractAlgorithm.ciBeaCleaner);

        [DispId(42)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void ClearBackground([In] double ThrLevel = 30.0);

        [DispId(43)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void Resize(
            [In] EPageSize PageSize,
            [In] EPageOrientation PageOrientation = EPageOrientation.epgoAuto,
            [In] EImageAlignment ImageAlignment = EImageAlignment.epiaCenter,
            [In] double Width = 8.5,
            [In] double Height = 11.0,
            [In] ESizeUnit Unit = ESizeUnit.esuInch);

        [DispId(44)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern EBoolean IsBlankImage([In] int reserved0 = 0, [In] double reserved1 = 0.0,
            [In] double reserved2 = 0.0);

        [TypeLibFunc(1088)]
        [DispId(45)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern int get_cx2l([In] int int_0);

        [DispId(45)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void set_cx2l([In] int int_0, [In] int value);

        [DispId(46)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern double get_cx3d([In] int int_0);

        [DispId(46)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void set_cx3d([In] int int_0, [In] double value);

        [DispId(47)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void AdvancedBinarize([In] int targetDpi = 0, [In] double reserved1 = 0.0,
            [In] double reserved2 = 0.0);

        [TypeLibFunc(1088)]
        [DispId(48)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void MinimizeBitsPerPixel([In] double reserved1 = 0.0, [In] double reserved2 = 0.0);

        [DispId(50)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void CleanNoiseExt(
            [In] ECleanNoiseFlags Flags,
            [In] int maxNoiseSizeHorz,
            [In] int maxNoiseSizeVert,
            [In] int minObjectDistance,
            [In] int reserved0);
    }
}