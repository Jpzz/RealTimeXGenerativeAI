import time
import os
import shutil
import cv2

def copy_images(image_path, output_path):
    shutil.copy2(image_path, output_path)

def merge_rgb_alpha(rgb_path, alpha_path, output_path):
    # RGB 이미지와 알파 마스크 이미지 읽기
    bgr_img = cv2.imread(rgb_path) # BGR로 읽힘
    if bgr_img is None:
        print(f"RGB 이미지를 불러오는데 실패했습니다: {rgb_path}")
        return
        
    alpha_img = cv2.imread(alpha_path, cv2.IMREAD_GRAYSCALE)
    if alpha_img is None:
        print(f"알파 이미지를 불러오는데 실패했습니다: {alpha_path}")
        return
    rgba = cv2.cvtColor(bgr_img, cv2.COLOR_BGR2BGRA)
    
    # 알파 채널에 마스크 할당
    rgba[:, :, 3] = alpha_img
    
    # 이미지 저장 성공 여부 확인
    result = cv2.imwrite(output_path, rgba)
    if not result:
        print(f"이미지 저장에 실패했습니다: {output_path}")

def check_folder_changes(folder_path, last_count):
    """Check if the number of files in the folder has changed"""
    try:
        current_count = len([f for f in os.listdir(folder_path) if os.path.isfile(os.path.join(folder_path, f))])
        if current_count > last_count:
            return True, current_count
        return False, current_count
    except:
        return False, 0

def monitor_folder(folder_path, callback_fn, check_interval=0.5):
    """Monitor the folder"""
    last_count = len([f for f in os.listdir(folder_path) if os.path.isfile(os.path.join(folder_path, f))])
    
    while True:
        changed, current_count = check_folder_changes(folder_path, last_count)
        if changed:
            callback_fn()
            print('Execute Callback Function')
            last_count = current_count
        time.sleep(check_interval)
