@echo off
call genAI\Scripts\activate.bat
set server="127.0.0.1:8000"
set seed=1280
set output_path="E:/unityproject/RealTimeXGenerativeAI/GenerativeAI/Assets/StreamingAssets"
set comfy_path="E:/generativeAI/ComfyUI-Desktop/ComfyUI"
set equipments="gloves"
set controlnet_image="gloves_controlnet.png"
python websocket_api_item.py --server %server% --seed %seed% --comfy_path %comfy_path% --output_path %output_path% --equipments %equipments% --controlnet_image %controlnet_image%