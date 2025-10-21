import cv2
import numpy as np

# === 1. 生成一张测试图像（包含浅变深边缘） ===
def create_test_image(width=400, height=300):
    # 创建渐变背景
    gradient = np.tile(np.linspace(50, 200, width, dtype=np.uint8), (height, 1))
    # 添加一个浅灰色圆形（边缘渐变）
    img = gradient.copy()
    cv2.circle(img, (200, 150), 80, 180, -1, lineType=cv2.LINE_AA)
    cv2.GaussianBlur(img, (15, 15), 0, dst=img)
    return img

# 生成图像
src = create_test_image()

# === 2. 普通 Canny ===
canny = cv2.Canny(src, 100, 200)

# === 3. CLAHE + Canny ===
clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8,8))
clahe_img = clahe.apply(src)
canny_clahe = cv2.Canny(clahe_img, 50, 150)

# === 4. LoG (Laplacian of Gaussian) ===
blur = cv2.GaussianBlur(src, (5, 5), 0)
log = cv2.Laplacian(blur, cv2.CV_16S, ksize=5)
log = cv2.convertScaleAbs(log)

# === 5. 拼接显示 ===
top = np.hstack((src, canny))
bottom = np.hstack((canny_clahe, log))
result = np.vstack((top, bottom))

# 添加文字标签
cv2.putText(result, "Original", (20, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, 255, 2)
cv2.putText(result, "Canny", (src.shape[1] + 20, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, 255, 2)
cv2.putText(result, "CLAHE + Canny", (20, src.shape[0] + 30), cv2.FONT_HERSHEY_SIMPLEX, 1, 255, 2)
cv2.putText(result, "LoG", (src.shape[1] + 20, src.shape[0] + 30), cv2.FONT_HERSHEY_SIMPLEX, 1, 255, 2)

# === 6. 显示结果 ===
cv2.imshow("Edge Detection Comparison", result)
cv2.imwrite("edge_compare_python.jpg", result)
cv2.waitKey(0)
cv2.destroyAllWindows()
