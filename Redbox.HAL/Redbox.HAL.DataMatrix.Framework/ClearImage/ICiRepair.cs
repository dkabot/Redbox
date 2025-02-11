using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage
{
    [Guid("63F6480C-997E-4FDE-AD63-A24E5F0FFDC7")]
    [TypeLibType(4160)]
    [ComVisible(true)]
    [ComImport]
    public interface ICiRepair
    {
        [DispId(1)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void FaxStandardToFine();

        [DispId(2)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void FaxRemoveBlankLines();

        [DispId(3)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void FaxRemoveHeader();

        [DispId(36)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        double AutoDeskew();

        [DispId(37)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        EPageRotation AutoRotate();

        [DispId(39)]
        CiImage Image
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
        int pMinLineLength
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
        int pMaxLineGap
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
        double pMaxLineAngle
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
        ELineCurvature pLineCurvature
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
        void CleanBorders();

        [DispId(21)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void RemovePunchHoles();

        [DispId(22)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void RemoveHalftone();

        [DispId(23)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SmoothCharacters([In] ESmoothType Type);

        [DispId(24)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void CleanNoise([In] int NoiseSize);

        [DispId(25)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void AutoInvertImage([In] int Threshold);

        [TypeLibFunc(1088)]
        [DispId(26)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void AutoInvertBlocks([In] int MinWidth, [In] int MinHeight);

        [DispId(29)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void ReconstructLines([In] ELineDirection Direction);

        [DispId(30)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void AutoCrop([In] int NewLeftMargin, [In] int NewTopMargin, [In] int NewRightMargin, [In] int NewBottomMargin);

        [DispId(31)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void AutoRegister([In] int NewLeftMargin, [In] int NewTopMargin);

        [DispId(40)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void DeleteLines([In] ELineDirection Direction, [In] EBoolean bRepair = EBoolean.ciTrue);

        [DispId(41)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void BorderExtract([In] EBorderExtractFlags Flags = EBorderExtractFlags.ciBexBorderDeskewCrop,
            [In] EBorderExtractAlgorithm Algorithm = EBorderExtractAlgorithm.ciBeaCleaner);

        [DispId(42)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void ClearBackground([In] double ThrLevel = 30.0);

        [DispId(43)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Resize(
            [In] EPageSize PageSize,
            [In] EPageOrientation PageOrientation = EPageOrientation.epgoAuto,
            [In] EImageAlignment ImageAlignment = EImageAlignment.epiaCenter,
            [In] double Width = 8.5,
            [In] double Height = 11.0,
            [In] ESizeUnit Unit = ESizeUnit.esuInch);

        [DispId(44)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        EBoolean IsBlankImage([In] int reserved0 = 0, [In] double reserved1 = 0.0, [In] double reserved2 = 0.0);

        [DispId(45)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int get_cx2l([In] int int_0);

        [TypeLibFunc(1088)]
        [DispId(45)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void set_cx2l([In] int int_0, [In] int value);

        [TypeLibFunc(1088)]
        [DispId(46)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        double get_cx3d([In] int int_0);

        [TypeLibFunc(1088)]
        [DispId(46)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void set_cx3d([In] int int_0, [In] double value);

        [DispId(47)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void AdvancedBinarize([In] int targetDpi = 0, [In] double reserved1 = 0.0, [In] double reserved2 = 0.0);

        [DispId(48)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void MinimizeBitsPerPixel([In] double reserved1 = 0.0, [In] double reserved2 = 0.0);

        [DispId(50)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void CleanNoiseExt(
            [In] ECleanNoiseFlags Flags,
            [In] int maxNoiseSizeHorz,
            [In] int maxNoiseSizeVert,
            [In] int minObjectDistance,
            [In] int reserved0);
    }
}