using OpenCvSharp;
using System;

namespace TulipAlg.Core3D
{
    /// <summary>
    /// 纹理混合模式
    /// </summary>
    public enum BlendMode
    {
        /// <summary>Alpha混合模式</summary>
        Alpha,
        /// <summary>乘法混合模式</summary>
        Multiply,
        /// <summary>叠加混合模式</summary>
        Overlay,
        /// <summary>屏幕混合模式</summary>
        Screen,
        /// <summary>添加混合模式</summary>
        Add
    }

    /// <summary>
    /// 热度图纹理混合器
    /// </summary>
    public class HeatmapTextureBlender
    {
        /// <summary>
        /// 混合热度图和纹理
        /// </summary>
        /// <param name="heatBgr">BGR格式的热度图 (CV_8UC3)</param>
        /// <param name="texture">归一化纹理 (CV_32FC1, 范围0-1)</param>
        /// <param name="mode">混合模式</param>
        /// <param name="alpha">混合强度 (0-1)</param>
        /// <returns>混合后的BGR图像</returns>
        public static Mat Blend(Mat heatBgr, Mat texture, BlendMode mode = BlendMode.Alpha, double alpha = 0.5)
        {
            if (heatBgr.Width != texture.Width || heatBgr.Height != texture.Height)
            {
                throw new ArgumentException("热度图和纹理尺寸必须相同");
            }

            // 转换热度图为float [0, 1]
            using var heat = new Mat();
            heatBgr.ConvertTo(heat, MatType.CV_32FC3, 1.0 / 255.0);

            // 将单通道纹理扩展为三通道
            using var texture3 = new Mat();
            Cv2.Merge(new[] { texture, texture, texture }, texture3);

            Mat result;
            switch (mode)
            {
                case BlendMode.Alpha:
                    result = BlendAlpha(heat, texture3, alpha);
                    break;
                case BlendMode.Multiply:
                    result = BlendMultiply(heat, texture3, alpha);
                    break;
                case BlendMode.Overlay:
                    result = BlendOverlay(heat, texture3);
                    break;
                case BlendMode.Screen:
                    result = BlendScreen(heat, texture3, alpha);
                    break;
                case BlendMode.Add:
                    result = BlendAdd(heat, texture3, alpha);
                    break;
                default:
                    throw new ArgumentException($"不支持的混合模式: {mode}");
            }

            // 转换回8位BGR
            var output = new Mat();
            result.ConvertTo(output, MatType.CV_8UC3, 255.0);
            result.Dispose();

            return output;
        }

        /// <summary>
        /// Alpha混合：简单的线性插值
        /// </summary>
        private static Mat BlendAlpha(Mat heat, Mat texture3, double alpha)
        {
            var result = new Mat();
            // result = heat * (1 - alpha * texture3) + alpha * texture3
            using var temp1 = new Mat();
            using var temp2 = new Mat();
            
            Cv2.Multiply(texture3, Scalar.All(alpha), temp1);
            Cv2.Subtract(Scalar.All(1.0), temp1, temp2);
            Cv2.Multiply(heat, temp2, result);
            Cv2.Add(result, temp1, result);

            return result;
        }

        /// <summary>
        /// 乘法混合：纹理作为亮度调制
        /// </summary>
        private static Mat BlendMultiply(Mat heat, Mat texture3, double factor)
        {
            var result = new Mat();
            // result = heat * (0.6 + 0.4 * texture3) 可调整系数避免过暗
            using var temp = new Mat();
            
            Cv2.Multiply(texture3, Scalar.All(0.4), temp);
            Cv2.Add(temp, Scalar.All(0.6), temp);
            Cv2.Multiply(heat, temp, result);

            return result;
        }

        /// <summary>
        /// 叠加混合：结合multiply和screen效果
        /// </summary>
        private static Mat BlendOverlay(Mat heat, Mat texture3)
        {
            var result = new Mat(heat.Size(), heat.Type());

            var heatIndexer = heat.GetGenericIndexer<Vec3f>();
            var texIndexer = texture3.GetGenericIndexer<Vec3f>();
            var resultIndexer = result.GetGenericIndexer<Vec3f>();
            
            for (int y = 0; y < heat.Height; y++)
            {
                for (int x = 0; x < heat.Width; x++)
                {
                    var h = heatIndexer[y, x];
                    var t = texIndexer[y, x];
                    var o = new Vec3f();

                    for (int c = 0; c < 3; c++)
                    {
                        if (h[c] <= 0.5f)
                        {
                            o[c] = 2 * h[c] * t[c];
                        }
                        else
                        {
                            o[c] = 1 - 2 * (1 - h[c]) * (1 - t[c]);
                        }
                    }

                    resultIndexer[y, x] = o;
                }
            }

            return result;
        }

        /// <summary>
        /// 屏幕混合：提亮效果
        /// </summary>
        private static Mat BlendScreen(Mat heat, Mat texture3, double alpha)
        {
            var result = new Mat();
            // result = 1 - (1 - heat) * (1 - texture3)
            using var invHeat = new Mat();
            using var invTex = new Mat();
            using var temp = new Mat();
            
            Cv2.Subtract(Scalar.All(1.0), heat, invHeat);
            Cv2.Subtract(Scalar.All(1.0), texture3, invTex);
            Cv2.Multiply(invHeat, invTex, temp);
            Cv2.Subtract(Scalar.All(1.0), temp, result);

            // 与原图混合
            Cv2.AddWeighted(heat, 1.0 - alpha, result, alpha, 0, result);

            return result;
        }

        /// <summary>
        /// 加法混合：简单相加
        /// </summary>
        private static Mat BlendAdd(Mat heat, Mat texture3, double alpha)
        {
            var result = new Mat();
            using var temp = new Mat();
            
            Cv2.Multiply(texture3, Scalar.All(alpha), temp);
            Cv2.Add(heat, temp, result);
            
            // 确保不超过1.0
            using var clipped = new Mat();
            Cv2.Min(result, Scalar.All(1.0), result);

            return result;
        }

        /// <summary>
        /// 批量混合：一次生成多种混合效果
        /// </summary>
        /// <param name="heatBgr">热度图</param>
        /// <param name="texture">纹理</param>
        /// <param name="modes">要生成的混合模式列表</param>
        /// <param name="alpha">混合强度</param>
        /// <returns>混合结果字典</returns>
        public static Dictionary<BlendMode, Mat> BlendMultiple(
            Mat heatBgr, Mat texture, BlendMode[] modes, double alpha = 0.5)
        {
            var results = new Dictionary<BlendMode, Mat>();
            
            foreach (var mode in modes)
            {
                results[mode] = Blend(heatBgr, texture, mode, alpha);
            }

            return results;
        }

        /// <summary>
        /// 创建对比图：将多个图像水平拼接
        /// </summary>
        /// <param name="images">图像列表</param>
        /// <returns>拼接后的图像</returns>
        public static Mat CreateComparison(params Mat[] images)
        {
            if (images.Length == 0)
                throw new ArgumentException("至少需要一张图像");

            Mat result = new Mat();
            Cv2.HConcat(images, result);
            return result;
        }
    }
}
