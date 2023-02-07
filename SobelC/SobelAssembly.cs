using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SobelC
{
    public class SobelAssembly
    {

        [DllImport("../../../x64/Debug/AsemblerProject.dll")]
        public static extern int putSobelMasksH(int[]array);

        public static int ConvolutionFilter2Version(Bitmap sourceImage,
double[,] xkernel,
double[,] ykernel,
out Bitmap resultImage,
int threadAmount)
        {

            int width = sourceImage.Width;
            int height = sourceImage.Height;

            BitmapData srcData = sourceImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            //Get the total number of bytes in your image
            int bytes = srcData.Stride * srcData.Height;

         
            byte[] pixelBuffer = new byte[bytes]; //Img in byte array
            byte[] resultBuffer = new byte[bytes]; // Result img byte array
            byte[] pixelBufferShort = new byte[bytes / 4]; //Shortened-grayscale img byte array

           
            IntPtr srcScan0 = srcData.Scan0;

        
            Marshal.Copy(srcScan0, pixelBuffer, 0, bytes);

           
            sourceImage.UnlockBits(srcData);

            //Convert image to grayscale

            float rgb = 0;
            int j = 0;
            for (int i = 0; i < pixelBuffer.Length; i += 4)
            {
                rgb = pixelBuffer[i] * .21f;
                rgb += pixelBuffer[i + 1] * .71f;
                rgb += pixelBuffer[i + 2] * .071f;
       

                pixelBufferShort[j] = (byte)rgb;
                j++;
            }

            int newStride = srcData.Stride / 4;

            //This is how much your center pixel is offset from the border of your kernel
            //Sobel is 3x3, so center is 1 pixel from the kernel border
            int filterOffset = 1;
        
            var watch = System.Diagnostics.Stopwatch.StartNew();
        
            Parallel.For(filterOffset, height - filterOffset, new ParallelOptions { MaxDegreeOfParallelism = threadAmount }, (OffsetY) =>
            {
                for (int OffsetX = filterOffset; OffsetX < width - filterOffset; OffsetX++)
                {
                    //reset gradient value to 0
      
                    double mG = 0;

                    //position of the kernel center pixel
                    var byteOffsetPara = OffsetY * newStride + OffsetX;

                    int[] tableOfNine= new int[9];
                    int counter = 0;

                    //kernel calculations
                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            int calcOffset = byteOffsetPara + filterX + filterY * newStride;                       
                            tableOfNine[counter] = pixelBufferShort[calcOffset];
                            counter++;
                        }
                    }

                    //vector gradient value for this pixel

                    mG = Math.Sqrt(putSobelMasksH(tableOfNine));
                    
                    //set limits, bytes can hold values from 0 up to 255;
                    if (mG > 255) mG = 255;
                    else if (mG < 0) mG = 0;

                    //set new data in the other byte array for your image data
                    resultBuffer[byteOffsetPara * 4] = (byte)(mG);
                    resultBuffer[byteOffsetPara * 4 + 1] = (byte)(mG);
                    resultBuffer[byteOffsetPara * 4 + 2] = (byte)(mG);
                    resultBuffer[byteOffsetPara * 4 + 3] = 255;
                }

            }
            );

            watch.Stop();
         
            resultImage = new Bitmap(width, height);

            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
 
            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);

            resultImage.UnlockBits(resultData);

            resultImage.Save("../../../Graphics/GraphicsAfter.jpeg");

            return (int)watch.ElapsedTicks;
        }

        //Sobel operator kernel for horizontal pixel changes
        public static double[,] xSobel
        {
            get
            {
                return new double[,]
                {
            { -1, 0, 1 },
            { -2, 0, 2 },
            { -1, 0, 1 }
                };
            }
        }

        //Sobel operator kernel for vertical pixel changes
        public static double[,] ySobel
        {
            get
            {
                return new double[,]
                {
            {  1,  2,  1 },
            {  0,  0,  0 },
            { -1, -2, -1 }
                };
            }
        }
    }
}
