using System;
using ClearImage;

namespace RBDM
{
    public class clsRBDM
    {
        private const int int_0 = 32;
        private const int int_1 = 64;
        private const int int_2 = 128;
        private const int int_3 = 1;
        private const int int_4 = 8192;
        private const int int_5 = 5;
        private const int int_6 = 4;
        public CiBarcodes Barcodes;
        public CiServer Ci;
        private CiAdvColor ciAdvColor_0;
        private CiDataMatrix ciDataMatrix_0;
        private CiImage ciImage_0;
        private CiTools ciTools_0;
        public CiImage Image;
        private int int_7;

        public int Find(int maxBcIn)
        {
            if (Ci == null)
                throw new Exception("RBDM Server is not intialized");
            if (Image == null)
                throw new Exception("RBDM Image is not intialized");
            method_0();
            var result = 0;
            try
            {
                var str = Ci.get_Info((EInfoType)7745, 1061);
                var s = Ci.get_Info((EInfoType)7746);
                result = 0;
                int.TryParse(s, out result);
                str = Ci.get_Info((EInfoType)7747, result ^ 1061);
                ciTools_0 = Ci.CreateTools();
                ciAdvColor_0 = Ci.CreateAdvColor();
                int_7 = maxBcIn;
                return method_5(Image);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ciTools_0 = null;
                ciAdvColor_0 = null;
                Ci.get_Info((EInfoType)7748, result ^ 1061);
                if (ciImage_0 != null)
                    ciImage_0.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private bool method_0()
        {
            if (ciDataMatrix_0 != null)
                return true;
            try
            {
                ciDataMatrix_0 = Ci.CreateDataMatrix();
                return true;
            }
            catch (Exception ex)
            {
                ciDataMatrix_0 = null;
                throw ex;
            }
        }

        private bool method_1(CiRect ciRect_0, long long_0, long long_1)
        {
            return long_0 >= ciRect_0.left && long_0 <= ciRect_0.right && long_1 >= ciRect_0.Int32_0 &&
                   long_1 <= ciRect_0.bottom;
        }

        private bool method_2(CiBarcode ciBarcode_0, CiImage ciImage_1)
        {
            foreach (ICiBarcode barcode in Barcodes)
                if (method_1(barcode.Rect, (ciBarcode_0.Rect.left + ciBarcode_0.Rect.right) / 2,
                        (ciBarcode_0.Rect.Int32_0 + ciBarcode_0.Rect.bottom) / 2))
                    return false;
            return true;
        }

        private long method_3(
            CiImage ciImage_1,
            int int_8,
            int int_9,
            int int_10,
            int int_11,
            int int_12)
        {
            switch (int_8)
            {
                case 64:
                    if (ciImage_1.BitsPerPixel > 1)
                    {
                        ciAdvColor_0.Image = ciImage_1;
                        ciAdvColor_0.ConvertToBitonal(EBiTonalConversion.ciBtcEdgeOne, int_9, int_10, int_11);
                    }

                    break;
                case 128:
                    if (ciImage_1.BitsPerPixel > 1)
                    {
                        ciAdvColor_0.Image = ciImage_1;
                        ciAdvColor_0.ConvertToBitonal(EBiTonalConversion.ciBtcLocalThr, int_9, int_10, int_11);
                    }

                    break;
            }

            if ((int_12 & 1) != 0)
            {
                ciTools_0.Image = ciImage_1;
                ciTools_0.Fatten(1, (EMorphDirections)8192);
            }

            ciDataMatrix_0.Image = ciImage_1;
            ciDataMatrix_0.Find(int_7);
            foreach (CiBarcode barcode in ciDataMatrix_0.Barcodes)
                if (method_2(barcode, ciImage_1))
                    Barcodes.Add(barcode);
            return Barcodes.Count;
        }

        private int method_4(CiImage ciImage_1)
        {
            method_3(ciImage_1, 32, 0, 0, 0, 0);
            if (int_7 != 0 && Barcodes.Count >= int_7)
                return Barcodes.Count;
            method_3(ciImage_1, 128, 0, 50, 0, 0);
            if (int_7 != 0 && Barcodes.Count >= int_7)
                return Barcodes.Count;
            method_3(ciImage_1, 32, 0, 0, 0, 1);
            return int_7 != 0 && Barcodes.Count >= int_7 ? Barcodes.Count : Barcodes.Count;
        }

        private int method_5(CiImage ciImage_1)
        {
            var ciImage_1_1 = (CiImage)null;
            Barcodes = Ci.CreateBarcodes();
            try
            {
                ciDataMatrix_0.Image = ciImage_1;
                ciImage_1_1 = ciImage_1.Duplicate();
                ciAdvColor_0.Image = ciImage_1_1;
                ciAdvColor_0.ConvertToGrayscale();
                method_4(ciImage_1_1);
                if (int_7 != 0 && Barcodes.Count >= int_7)
                    return Barcodes.Count;
                ciTools_0.Image = ciImage_1_1;
                ciTools_0.Skew(5.0);
                method_4(ciImage_1_1);
                return int_7 != 0 && Barcodes.Count >= int_7 ? Barcodes.Count : Barcodes.Count;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (ciImage_1_1 != null && ciImage_1_1.IsValid == EBoolean.ciTrue)
                    ciImage_1_1.Close();
                if (ciDataMatrix_0 != null && ciDataMatrix_0.Image.IsValid == EBoolean.ciTrue)
                    ciDataMatrix_0.Image.Close();
                if (ciTools_0 != null && ciTools_0.Image.IsValid == EBoolean.ciTrue)
                    ciTools_0.Image.Close();
                if (ciAdvColor_0 != null && ciAdvColor_0.Image.IsValid == EBoolean.ciTrue)
                    ciAdvColor_0.Image.Close();
            }
        }
    }
}