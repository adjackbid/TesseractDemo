using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Tesseract;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;

namespace TesseractDemo
{
    public class OCRHelper
    {
        public static string OCR(Bitmap img)
        {
            TesseractEngine ocr = null;
            string sResult = "";
            try
            {
                ocr = new TesseractEngine("./tessdata", "eng"); //初始化 (一定要放在tessdata資料夾下)
                ocr.SetVariable("tessedit_char_whitelist", "0123456789"); //強迫Char List，較準確

                Page page = ocr.Process(img, PageSegMode.SingleLine);
                sResult = page.GetText();//result
                page.Dispose();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                sResult = "";
            }
            finally
            {
                ocr?.Dispose();
            }
            return sResult.Replace(" ", "");
        }
    }
}
