# 带纹理的热度图示例（Python + OpenCV）
# 运行: pip install opencv-python numpy matplotlib
import cv2
import numpy as np
import matplotlib.pyplot as plt

def generate_scalar_field(shape=(400,400), centers=None):
    """生成示例标量场（几个高斯山峰）"""
    h, w = shape
    if centers is None:
        centers = [
            (w*0.3, h*0.3, 0.12),
            (w*0.7, h*0.4, 0.18),
            (w*0.5, h*0.75, 0.15)
        ]
    Y, X = np.mgrid[0:h, 0:w].astype(np.float32)
    field = np.zeros((h,w), dtype=np.float32)
    for cx, cy, sigma_rel in centers:
        sigma = min(h,w) * sigma_rel
        field += np.exp(-(((X-cx)**2 + (Y-cy)**2) / (2*sigma*sigma)))
    # 归一化到 [0,1]
    field = (field - field.min()) / (field.max() - field.min() + 1e-12)
    return field

def apply_colormap(field, cmap=cv2.COLORMAP_JET):
    """把归一化标量场变成 BGR 彩色热图（OpenCV 格式）"""
    img_8u = np.uint8(field * 255)
    heat = cv2.applyColorMap(img_8u, cmap)  # BGR
    return heat

def generate_noise_texture(shape, scale=8.0, octaves=4, seed=None):
    """
    生成分形噪声近似（简单多层高斯模糊的随机噪声）
    这是比 Perlin 简化的替代办法：合成若干频率的噪声层。
    """
    if seed is not None:
        np.random.seed(seed)
    h, w = shape
    base = np.zeros((h,w), dtype=np.float32)
    freq = 1.0
    amp = 1.0
    for _ in range(octaves):
        noise = np.random.randn(h, w).astype(np.float32)
        # 平滑每层噪声，模仿低频成分
        ksize = max(1, int(scale / freq))
        if ksize % 2 == 0: ksize += 1
        blurred = cv2.GaussianBlur(noise, (ksize, ksize), sigmaX=ksize/2)
        base += blurred * amp
        freq *= 2.0
        amp *= 0.5
    # 归一化到 [0,1]
    base = base - base.min()
    base = base / (base.max() + 1e-12)
    return base

def generate_hatch_texture(shape, spacing=12, angle_deg=30, thickness=1):
    """生成斜线 hatch 纹理（白底黑线），返回 float 0..1，其中1表示白/亮"""
    h, w = shape
    # 画到单通道黑底画白线，最后反转到 [0,1]
    canvas = np.zeros((h,w), dtype=np.uint8)
    # 创建一个更大的画布以方便旋转
    diag = int(np.hypot(h,w))
    big = np.zeros((diag, diag), dtype=np.uint8)
    # 间隔填线
    for x in range(-diag, diag, spacing):
        cv2.line(big, (x, 0), (x+diag, diag), color=255, thickness=thickness)
    # 旋转
    M = cv2.getRotationMatrix2D((diag/2, diag/2), angle_deg, 1.0)
    rotated = cv2.warpAffine(big, M, (diag, diag), flags=cv2.INTER_LINEAR)
    # 裁切中心区域
    startx = (diag - w)//2
    starty = (diag - h)//2
    cropped = rotated[starty:starty+h, startx:startx+w]
    # 归一化到 0..1（白为1）
    tex = cropped.astype(np.float32) / 255.0
    # 这里希望纹理是亮区为 1，暗区为 0；最终可用不同blend方式
    return tex

def blend_heat_with_texture(heat_bgr, texture, mode='alpha', alpha=0.5):
    """
    heat_bgr: HxWx3 uint8 (BGR)
    texture: HxW float in [0,1] (纹理亮度)
    mode: 'alpha' (叠加), 'multiply' (相乘), 'overlay' (近似)
    alpha: alpha 值用于 alpha 模式（纹理覆盖强度）
    """
    h, w = texture.shape
    heat = heat_bgr.astype(np.float32) / 255.0  # 0..1
    # 将纹理扩展到三通道
    tex3 = np.dstack([texture]*3)
    if mode == 'alpha':
        # 把纹理映射为灰色到黑色（即纹理作为暗化层）
        out = heat * (1.0 - alpha*tex3) + (alpha*tex3)  # 这是一个示例，你可调整公式
    elif mode == 'multiply':
        # 纹理作为乘积系数（暗化亮度）
        out = heat * (0.6 + 0.4*tex3)  # 保证不会完全变黑，可调
    elif mode == 'overlay':
        # 简单近似 overlay：亮部变亮，暗部变暗
        mask = heat <= 0.5
        out = heat.copy()
        out[mask] = 2 * heat[mask] * tex3[mask]
        out[~mask] = 1 - 2*(1-heat[~mask])*(1-tex3[~mask])
    else:
        raise ValueError("Unknown mode")
    out = np.clip(out, 0.0, 1.0)
    return (out * 255).astype(np.uint8)

# -------- 主流程示例 --------
if __name__ == "__main__":
    H, W = 480, 640
    field = generate_scalar_field((H, W))
    heat = apply_colormap(field, cmap=cv2.COLORMAP_INFERNO)  # 也可以COLORMAP_JET等

    # 1) 噪声纹理
    noise_tex = generate_noise_texture((H, W), scale=24.0, octaves=5, seed=42)
    # 2) 斜线纹理
    hatch_tex = generate_hatch_texture((H, W), spacing=18, angle_deg=35, thickness=2)

    # 混合
    blended_noise = blend_heat_with_texture(heat, noise_tex, mode='multiply')
    blended_hatch = blend_heat_with_texture(heat, hatch_tex, mode='alpha', alpha=0.5)

    # 组合展示
    top = np.hstack([heat, blended_noise, blended_hatch])
    # 用 matplotlib 显示（转换 BGR->RGB）
    top_rgb = cv2.cvtColor(top, cv2.COLOR_BGR2RGB)
    plt.figure(figsize=(12,6))
    plt.imshow(top_rgb)
    plt.axis('off')
    plt.title("原始热图 | 噪声纹理叠加(multiply) | 斜线纹理叠加(alpha)")
    plt.show()
