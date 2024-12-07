@echo off
call genAI\Scripts\activate.bat
set server="127.0.0.1:8000"
set output_path="E:/unityproject/RealTimeXGenerativeAI/GenerativeAI/Assets/StreamingAssets"
set comfy_path="E:/generativeAI/ComfyUI-Desktop/ComfyUI"
set segment_image="axe_0001.png"
python websocket_api_segment.py --server %server% --comfy_path %comfy_path% --output_path %output_path% --segment_image %segment_image%